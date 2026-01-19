using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;

namespace NodeSharp.Client.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class AnchorPoint : ObservableObject
{
    private readonly Input input;

    public AnchorPoint( BoxNode boxNode, Input input)
    {
        this.input = input;
        this.ConnectionType = connectionType;
        BoxNode = boxNode;
        X = x;
        Y = y;
    }
    
    public AnchorPoint(string id, double x, double y, BoxNode boxNode, InOrOutConnection connectionType)
    {
        this.ConnectionType = connectionType;
        BoxNode = boxNode;
        X = x;
        Y = y;
    }

    public AnchorPoint(IEnumerable<string> ids, double x, double y, BoxNode boxNode, InOrOutConnection connectionType)
    {
        this.ConnectionType = connectionType;
        BoxNode = boxNode;
        X = x;
        Y = y;

        foreach (var id in ids)
        {
        }
    }

    public double AbsoluteCenterX
    {
        get
        {
            if (ConnectionType == InOrOutConnection.In)
            {
                return BoxNode.X + X + 5; // X is relative to LeftAnchorArea, +5 centers 10px element
            }

            return BoxNode.X + BoxNode.Width + X; // No +5 needed since X already represents the center
        }
    }

    public double AbsoluteCenterY
    {
        get
        {
            if (ConnectionType == InOrOutConnection.In)
            {
                return BoxNode.Y + Y + (LayoutBounds.Height / 2.0);
            }

            return BoxNode.Y + Y + (LayoutBounds.Height / 2.0);
        }
    }

    public double AbsoluteX
    {
        get
        {
            if (ConnectionType == InOrOutConnection.In)
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
            if (ConnectionType == InOrOutConnection.In)
            {
                return BoxNode.Y + Y;
            }

            return BoxNode.Y + Y;
        }
    }

    [ObservableProperty] private InOrOutConnection connectionType;

    [ObservableProperty] private double x;

    [ObservableProperty] private double y;
    
    [ObservableProperty] private double canvasX;

    [ObservableProperty] private double canvasY;

    [ObservableProperty] private BoxNode boxNode;

    [ObservableProperty] private IList<string> ids = [];

    public Rect LayoutBounds
    {
        get
        {
            var width = 6;
            var height = 6;

            var rect = new Rect(X, Y - (height / 2.0), width, height);
            System.Diagnostics.Debug.WriteLine($"LayoutBounds: {rect}");
            return rect;
        }
    }
}