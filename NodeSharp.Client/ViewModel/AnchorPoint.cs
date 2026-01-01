using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Client.ViewModel;

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

    public double AbsoluteCenterX
    {
        get
        {
            if (connectionType == InOrOutConnection.In)
            {
                return BoxNode.X + (LayoutBounds.Width / 2.0);;
            }
            
            return BoxNode.X + BoxNode.Width + (LayoutBounds.Width / 2.0);;
        }
    }
    
    public double AbsoluteCenterY
    {
        get
        {
            if (connectionType == InOrOutConnection.In)
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
            var width = 6;
            var height = 6;
            
            var rect = new Rect(X, Y - (height / 2.0), width, height);
            System.Diagnostics.Debug.WriteLine($"LayoutBounds: {rect}");
            return rect;
        }
    }
}