using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Client.Component;
using NodeSharp.Nodes.Common;

namespace NodeSharp.Client.ViewModel;

public enum InOrOutConnection
{
    In = 1001,
    Out = 1002
}

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class BoxNode : ObservableObject
{
    public IList<AnchorPoint> InputNodes { get; } = [];
    public IList<AnchorPoint> OutputNodes { get; } = [];

    public BoxNode(BaseNode baseNode)
    {
        Node = baseNode;
        AbsolutePosition = new Point(0, 0);
        BoxColor = Colors.BlanchedAlmond;
        SetInitialPosition(baseNode);

        baseNode.OnEnterNode += (o, nodeRun) =>
        {
            Debug.WriteLine($"Node entered (Run): {nodeRun.Name} ({nodeRun.Id})");

            HasFocus = true;
        };

        baseNode.OnLeaveNode += (o, valueTuple) =>
        {
            var nodeRun = valueTuple.Item1;
            var stopWatch = valueTuple.Item2;
            var elapsedTime = stopWatch.ElapsedMilliseconds;

            Debug.WriteLine($"Node exited (Run): {nodeRun.Name} ({nodeRun.Id}::{elapsedTime}ms)");

            HasFocus = false;
        };
    }

    private void SetInitialPosition(BaseNode baseNode)
    {
        X = baseNode.X;
        Y = baseNode.Y;
    }


    partial void OnXChanged(double value)
    {
        XCenter = value - (Node.BoxDimension.Width / 2);
    }
    
    partial void OnYChanged(double value)
    {
        YCenter = value - (Node.BoxDimension.Height / 2);
    }
    
    // public Point PtCenter
    // {
    //     get => new(XCenter, YCenter);
    // }


    // public double YCenter
    // {
    //     get
    //     {
    //         if (Height > 0)
    //         {
    //             return (Height / 2) + Y;
    //         }
    //
    //         return Y;
    //     }
    // }
    //
    // public double XCenter
    // {
    //     get
    //     {
    //         if (Width > 0)
    //         {
    //             return (Width / 2) + X;
    //         }
    //
    //         return X;
    //     }
    // }

    [ObservableProperty] private Rect bounds;

    [ObservableProperty] private Point absolutePosition;

    //
    [ObservableProperty] private double x;

    [ObservableProperty] private double y;

    [ObservableProperty] private double xCenter;

    [ObservableProperty] private double yCenter;
    //
    // [ObservableProperty] private double width;
    //
    // [ObservableProperty] private double height;
    //
    // [ObservableProperty] private string nodeId;
    //
    // [ObservableProperty] private bool isEnabled;
    //
    [ObservableProperty] private Color boxColor;
    //
    [ObservableProperty] private BaseNode node;

    //
    [ObservableProperty] private DraggableBoxComponent? draggableBoxComponent;
    //
    // [ObservableProperty] private string name;

    [ObservableProperty] private bool hasFocus;

    // private void CalculateInputNodePositions()
    // {
    //     const double fromVerticalMargin = 8.0;
    //     
    //     InputNodes.Clear();
    //
    //     var inputs = this.Node.Inputs;
    //     var verticalStep = (Height - fromVerticalMargin) / (inputs.Count + 1);
    //     var yPositionDelta = (fromVerticalMargin / 2) + (verticalStep / 2);
    //
    //     for (var i = 0; i < inputs.Count; i++)
    //     {
    //         var fromSquareYPos = verticalStep * (i + 1);
    //         var ptInputSquare = new AnchorPoint(Node.Id, 0, fromSquareYPos, this, InOrOutConnection.In);
    //
    //         InputNodes.Add(ptInputSquare);
    //     }
    // }

    private void AddInputNodePositions()
    {
        InputNodes.Clear();

        var inputs = this.Node.Inputs;

        foreach (var input in inputs)
        {
            var ptInputSquare = new AnchorPoint(this, input);

            InputNodes.Add(ptInputSquare);
        }
    }

    private void AddOutputNodePositions()
    {
        {
            OutputNodes.Clear();

            var outputs = this.Node.Outputs;

            foreach (var output in outputs)
            {
                var ptInputSquare = new AnchorPoint(this, output);

                OutputNodes.Add(ptInputSquare);
            }
        }
    }

    partial void OnBoundsChanged(Rect value)
    {
        AddInputNodePositions();
        AddOutputNodePositions();
    }


    public void RebuildAnchorPointConnections(IList<BoxNode> boxNodes)
    {
        foreach (var inputNode in InputNodes)
        {
            inputNode.RebuildAnchorPointConnections(boxNodes);
        }
    }
}