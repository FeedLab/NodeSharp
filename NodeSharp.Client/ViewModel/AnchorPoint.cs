using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
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
    public AnchorPoint(BoxNode boxNode, Input input)
    {
        InputConnection = input;
        BoxNode = boxNode;

        ConnectionType = InOrOutConnection.In;
        X = input.StartPosition.X;
        Y = input.StartPosition.Y;

        nodeIo = AppService.GetRequiredService<NodeIo>();

        // input.ConnectsToParentNodeId.CollectionChanged += (sender, args) =>
        // {
        //     var item = (string)args.NewItems?[0]!;
        //
        //     switch(args.Action)
        //     {
        //         case NotifyCollectionChangedAction.Add:
        //             ConnectsToNodeId.Add(item);
        //             break;
        //         case NotifyCollectionChangedAction.Remove:
        //             ConnectsToNodeId.Remove(item);
        //             break;
        //         case NotifyCollectionChangedAction.Replace:
        //             break;
        //         case NotifyCollectionChangedAction.Move:
        //             break;
        //         case NotifyCollectionChangedAction.Reset:
        //             ConnectsToNodeId.Clear();
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // };
    }

    public AnchorPoint(BoxNode boxNode, Output output)
    {
        OutputConnection = output;
        BoxNode = boxNode;

        ConnectionType = InOrOutConnection.Out;
        X = output.StartPosition.X;
        Y = output.StartPosition.Y;
        
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
            
            if (anchorComponent is VisualElement ve)
            {
                ve.Loaded += OnLoaded;
            }

            anchorComponent = value;

            
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
            CalculateAbsolutePosition();
    }

    private void CalculateAbsolutePosition()
    {
        CanvasSurface = anchorComponent?.GetVisualAncestors().OfType<AbsoluteLayout>()
            .Single(s => s.AutomationId == "CanvasSurface");

        if (CanvasSurface is null)
        {
            throw new InvalidOperationException("CanvasSurface not found for anchor component");
        }
            
        RelativePosition = anchorComponent?.GetRelativePosition(CanvasSurface);
    }

    public Input? InputConnection { get; }
    public Output? OutputConnection { get; }
    private IVisualTreeElement? CanvasSurface { get; set; }
    
    [ObservableProperty] private InOrOutConnection connectionType;

    [ObservableProperty] private double x;

    [ObservableProperty] private double y;

    [ObservableProperty] private double canvasX;

    [ObservableProperty] private double canvasY;

    [ObservableProperty] private BoxNode boxNode;

    [ObservableProperty] private Point? relativePosition;

    private IVisualTreeElement? anchorComponent;
    
    private readonly NodeIo nodeIo;
    
    [ObservableProperty] private ObservableCollection<AnchorPoint> connectsToNodeId = [];
    

    public void AddConnection(AnchorPoint anchorPoint)
    {
            
    }

    public void RebuildAnchorPointConnections(IList<BoxNode> boxNodes)
    {
        // var inputNodeList = nodeIo.Nodes.SelectMany(s => s.Inputs).ToList();
        // var outputNodeList = nodeIo.Nodes.SelectMany(s => s.Outputs).ToList();
        var inputAnchorPoints = boxNodes.SelectMany(s => s.InputNodes).ToList();
        var outputAnchorPoints = boxNodes.SelectMany(s => s.OutputNodes).ToList();

        ConnectsToNodeId.Clear();

        if (InputConnection is not null)
        {

            foreach (var id in InputConnection.ConnectsToParentNodeId)
            {
                var outputNode = outputAnchorPoints.Single(outputAnchorPoint => outputAnchorPoint.OutputConnection?.Id.ToString() == id);
                
                ConnectsToNodeId.Add(outputNode);
            }   
        }
        else if(OutputConnection is not null)
        {
            foreach (var id in OutputConnection.ConnectsToNodeId)
            {
                var inputNode = inputAnchorPoints.Single(inputAnchorPoint => inputAnchorPoint.InputConnection?.Id.ToString() == id);
                
                ConnectsToNodeId.Add(inputNode);
            }
        }
        else
        {
            throw new InvalidOperationException("AnchorPoint must be connected to either an Input or Output.");
        }
    }
}