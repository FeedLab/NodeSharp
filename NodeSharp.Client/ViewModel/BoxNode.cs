using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.ViewModel;

public enum InOrOutConnection
{
    In = 1001,
    Out = 1002
}

public partial class AnchorPoint : ObservableObject
{
    private readonly InOrOutConnection connectionType;

    public AnchorPoint(string id, double x, double y, BoxNode boxNode, InOrOutConnection connectionType)
    {
        this.connectionType = connectionType;
        BoxNode= boxNode;
        X = x;
        Y = y;
        
        Ids.Add(id);
    }
    
    public AnchorPoint(IEnumerable<string> ids, double x, double y, BoxNode boxNode, InOrOutConnection connectionType)
    {
        this.connectionType = connectionType;
        BoxNode = boxNode;
        X = x;
        Y = y;

        foreach (var id in ids)
        {
            Ids.Add(id);
        }
    }

    public double AbsoluteX
    {
        get
        {
            if (connectionType == InOrOutConnection.In)
            {
                return BoxNode.X;
            }
            
            return BoxNode.X + BoxNode.Width;
        }
    }
    
    public double AbsoluteY
    {
        get
        {
            if (connectionType == InOrOutConnection.In)
            {
                return BoxNode.Y + Y;
            }
            
            return BoxNode.Y + Y;
        }
    }
    [ObservableProperty] 
    private double x;
    
    [ObservableProperty] 
    private double y;

    [ObservableProperty] 
    private BoxNode boxNode;
    
    [ObservableProperty]
    private IList<string> ids = [];
    
    public Rect LayoutBounds 
    { 
        get
        {
            var rect = new Rect(X, Y, 6, 6);
            System.Diagnostics.Debug.WriteLine($"LayoutBounds: {rect}");
            return rect;
        }
    }
}

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
    }

    // public AnchorPoint TransformInputToAbsolutePosition(AnchorPoint anchorPoint)
    // {
    //     return new AnchorPoint(anchorPoint.Ids, anchorPoint.X + X, anchorPoint.Y + Y);
    // }
    //
    // public AnchorPoint TransformOutPutToAbsolutePosition(AnchorPoint anchorPoint)
    // {
    //     return new AnchorPoint(anchorPoint.Ids, anchorPoint.X + X + Width, anchorPoint.Y + Y );
    // }
    
    // public IList<AnchorPoint> TransformedInputNodesToAbsolutePosition()
    // {
    //     return InputNodes.Select(s => new AnchorPoint(s.Ids, s.X + X, s.Y + Y)).ToList();
    // }
    //
    // public IList<AnchorPoint> TransformedOutputNodesToAbsolutePosition()
    // {
    //     return OutputNodes.Select(s => new AnchorPoint(s.Ids, s.X + X + Width, s.Y + Y)).ToList();
    // }
    
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

// [ObservableProperty] 
// private DiagramViewModel diagramViewModel;

    [ObservableProperty] private string name;
}