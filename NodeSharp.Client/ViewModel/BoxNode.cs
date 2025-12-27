using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.ViewModel;

public partial class BoxNode : ObservableObject
{
    public IList<(Point Start, Point End)> Connections { get; } = [];

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
            if (Height > 0)
            {
                return (Width / 2) + X;
            }

            return Y;
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