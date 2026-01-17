using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;

namespace NodeSharp.Client.ViewModel;

public enum InOrOutConnection
{
    In = 1001,
    Out = 1002
}

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class BoxNode : ObservableObject
{
    public IList<(Point Start, Point End)> Connections { get; } = [];
    public IList<AnchorPoint> InputNodes { get; } = [];
    public IList<AnchorPoint> OutputNodes { get; } = [];
    

    public BoxNode(BaseNode baseNode)
    {
        node = baseNode;
        // this.diagramViewModel = diagramViewModel;
        NodeId = node.Id;
        Name = node.Name;
        BoxColor = Colors.BlanchedAlmond;
        IsEnabled = node.IsEnabled;
        X = node.X;
        Y = node.Y;
        Width = 130;
        Height = 50;
        width = 0;
        
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

   
    public Point PtCenter
    {
        get => new(XCenter, YCenter);
    }


    public double YCenter
    {
        get
        {
            if (Height > 0)
            {
                return (Height / 2) + Y;
            }

            return Y;
        }
    }

    public double XCenter
    {
        get
        {
            if (Width > 0)
            {
                return (Width / 2) + X;
            }

            return X;
        }
    }
    
    [ObservableProperty] private double x;

    [ObservableProperty] private double y;

    [ObservableProperty] private double width;

    [ObservableProperty] private double height;

    [ObservableProperty] private string nodeId;

    [ObservableProperty] private bool isEnabled;

    [ObservableProperty] private Color boxColor;

    [ObservableProperty] private BaseNode node;

    [ObservableProperty] private string name;
    
    [ObservableProperty] private bool hasFocus;

    private void CalculateInputNodePositions()
    {
        const double fromVerticalMargin = 8.0;
        
        InputNodes.Clear();

        var inputs = this.Node.Inputs;
        var verticalStep = (Height - fromVerticalMargin) / (inputs.Count + 1);
        var yPositionDelta = (fromVerticalMargin / 2) + (verticalStep / 2);

        for (var i = 0; i < inputs.Count; i++)
        {
            var fromSquareYPos = verticalStep * (i + 1);
            var ptInputSquare = new AnchorPoint(Node.Id, 0, fromSquareYPos, this, InOrOutConnection.In);

            InputNodes.Add(ptInputSquare);
        }
    }
    
    private void CalculateOutputNodePositions()
    {
        const double verticalMargin = 8.0;

        OutputNodes.Clear();

        var outputs = Node.Outputs;
        var availableHeight = Height - verticalMargin;
        var verticalStep = availableHeight / (outputs.Count + 1);
        // var yPositionDelta = (verticalMargin / 2);
        var index = 1;
        foreach (var output in outputs)
        {
            var connectedIds = output.ConnectsToNodeId.ToList();
            var yPosition = (verticalStep * index);// - yPositionDelta;

            //    var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition + (verticalMargin / 2), boxNode,
            var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition, this, InOrOutConnection.Out);
            OutputNodes.Add(anchorPoint);

            index++;
        }
    }
    
    partial void OnHeightChanged(double value)
    {
       CalculateInputNodePositions();
       CalculateOutputNodePositions();
    }
}