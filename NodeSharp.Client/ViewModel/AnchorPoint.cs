using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
[SuppressMessage("Usage", "CsWinRT1030:Project does not enable unsafe blocks")]
public partial class AnchorPoint : ObservableObject
{
    public AnchorPoint(BoxNode boxNode, Input originalInput)
    {
        OriginalInput = originalInput;
        BoxNode = boxNode;
        
        ConnectionType = InOrOutConnection.In;
        Id = originalInput.Id.ToString();
        X = originalInput.StartPosition.X;
        Y = originalInput.StartPosition.Y;

        ConnectsToNodeId.CollectionChanged += ConnectsToNodeIdOnCollectionChanged;
        nodeIo = AppService.GetRequiredService<NodeIo>();
    }

    public AnchorPoint(BoxNode boxNode, Output originalOutput)
    {
        OriginalOutput = originalOutput;
        BoxNode = boxNode;

        ConnectionType = InOrOutConnection.Out;
        Id = originalOutput.Id.ToString();
        X = originalOutput.StartPosition.X;
        Y = originalOutput.StartPosition.Y;

        ConnectsToNodeId.CollectionChanged += ConnectsToNodeIdOnCollectionChanged;

        nodeIo = AppService.GetRequiredService<NodeIo>();
    }

    public IVisualTreeElement? AnchorComponent
    {
        get => anchorComponent;
        set
        {
            if (value is null)
            {
                var exception = new ArgumentNullException(nameof(anchorComponent))
                {
                    HelpLink = null,
                    HResult = 0,
                    Source = null
                };

                throw exception;
            }

            anchorComponent = value;
        }
    }

    public Input? OriginalInput { get; }
    public Output? OriginalOutput { get; }

    [ObservableProperty] private InOrOutConnection connectionType;

    [ObservableProperty] private double x;
    
    [ObservableProperty] private string id;

    [ObservableProperty] private double y;

    [ObservableProperty] private double canvasX;

    [ObservableProperty] private double canvasY;

    [ObservableProperty] private BoxNode boxNode;

    [ObservableProperty] private Point? relativePosition;

    private IVisualTreeElement? anchorComponent;

    private readonly NodeIo nodeIo;

    [ObservableProperty] private ObservableCollection<AnchorPoint> connectsToNodeId = [];

    partial void OnXChanged(double value)
    {
        switch (ConnectionType)
        {
            case InOrOutConnection.In when OriginalInput is not null:
                OriginalInput.StartPosition = new Point(value, OriginalInput.StartPosition.Y);
                break;
            case InOrOutConnection.Out when OriginalOutput is not null:
                OriginalOutput.StartPosition = new Point(value, OriginalOutput.StartPosition.Y);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    partial void OnYChanged(double value)
    {
        switch (ConnectionType)
        {
            case InOrOutConnection.In when OriginalInput is not null:
                OriginalInput.StartPosition = new Point(OriginalInput.StartPosition.X, value);
                break;
            case InOrOutConnection.Out when OriginalOutput is not null:
                OriginalOutput.StartPosition = new Point(OriginalOutput.StartPosition.X, value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ConnectsToNode(string nodeId)
    {
        switch (ConnectionType)
        {
            case InOrOutConnection.In when OriginalInput is not null:
            {
                OriginalInput.ConnectsToParentNodeId.Add(nodeId);
                break;
            }
            case InOrOutConnection.Out when OriginalOutput is not null:
            {
                OriginalOutput.ConnectsToNodeId.Add(nodeId);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ConnectsToNodeIdOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // switch (e.Action)
        // {
        //     case NotifyCollectionChangedAction.Add:
        //         UpdateConnectionIds();
        //         break;
        //     case NotifyCollectionChangedAction.Remove:
        //         break;
        //     case NotifyCollectionChangedAction.Replace:
        //         break;
        //     case NotifyCollectionChangedAction.Move:
        //         break;
        //     case NotifyCollectionChangedAction.Reset:
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
        // 
    }

    // private void InitializeConnectionIds()
    // {
    //     switch (ConnectionType)
    //     {
    //         case InOrOutConnection.In when OriginalInput is not null:
    //         {
    //             var connectionIds = OriginalInput.ConnectsToParentNodeId
    //                 .Select(s => s)
    //                 .ToList();
    //
    //             
    //             ConnectsToNodeId = new ObservableCollection<AnchorPoint>(connectionIds);
    //             break;
    //         }
    //         case InOrOutConnection.Out when OriginalOutput is not null:
    //         {
    //             var connectionIds = ConnectsToNodeId
    //                 .Select(s => s.Id.ToString())
    //                 .ToList();
    //
    //             OriginalOutput.ConnectsToNodeId = new ObservableCollection<string>(connectionIds);
    //             break;
    //         }
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
    
    private void UpdateConnectionIds()
    {
        switch (ConnectionType)
        {
            case InOrOutConnection.In when OriginalInput is not null:
            {
                var connectionIds = ConnectsToNodeId
                    .Select(s => s.Id.ToString())
                    .ToList();

                OriginalInput.ConnectsToParentNodeId = new ObservableCollection<string>(connectionIds);
                break;
            }
            case InOrOutConnection.Out when OriginalOutput is not null:
            {
                var connectionIds = ConnectsToNodeId
                    .Select(s => s.Id.ToString())
                    .ToList();

                OriginalOutput.ConnectsToNodeId = new ObservableCollection<string>(connectionIds);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RebuildAnchorPointConnections(IList<BoxNode> boxNodes)
    {
        var inputAnchorPoints = boxNodes.SelectMany(s => s.InputNodes).ToList();
        var outputAnchorPoints = boxNodes.SelectMany(s => s.OutputNodes).ToList();

        ConnectsToNodeId.Clear();

        if (OriginalInput is not null)
        {
            foreach (var id in OriginalInput.ConnectsToParentNodeId)
            {
                var outputNode = outputAnchorPoints.Single(outputAnchorPoint =>
                    outputAnchorPoint.OriginalOutput?.Id.ToString() == id);

                ConnectsToNodeId.Add(outputNode);
            }
        }
        else if (OriginalOutput is not null)
        {
            foreach (var id in OriginalOutput.ConnectsToNodeId)
            {
                var inputNode = inputAnchorPoints.Single(inputAnchorPoint =>
                    inputAnchorPoint.OriginalInput?.Id.ToString() == id);

                ConnectsToNodeId.Add(inputNode);
            }
        }
        else
        {
            throw new InvalidOperationException("AnchorPoint must be connected to either an Input or Output.");
        }
    }

    public void SetRelativePosition(Point positionPt, double width, double height)
    {
        if (ConnectionType == InOrOutConnection.Out)
        {
            positionPt.X += width;
            positionPt.Y += height / 2;
            RelativePosition = positionPt;
        }
        else
        {
            positionPt.X += 0;
            positionPt.Y += height / 2;
            RelativePosition = positionPt;
        }

        RelativePosition = positionPt;
    }
}