using System.Collections.ObjectModel;
using NodeSharp.Client.Component;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.ViewModel;

public class DiagramViewModel(Main main)
{
    private const string BaseFilePath = ".";
    public ObservableCollection<BoxNode> Nodes { get; } = new();
    
    public async Task Init()
    {
        var fileToLoad = $"{BaseFilePath}\\Nodes.json";

       await main.LoadFromFileAsync(fileToLoad);

       Nodes.Clear();
       
       foreach (var node in main.Nodes)
       {
           var boxNode = new BoxNode
           {
               Node = node,
               Id = node.Id,
               Name = node.Name,
               BoxColor = Colors.BlanchedAlmond,
               IsEnabled = node.IsEnabled,
               X = 100,
               Y = 100,
               Width = 130,
               Height = 50
           };

           Nodes.Add(boxNode);
       }
    }
}

public class BoxNode
{
    public BoxNode()
    {
        BoxColor = Colors.ForestGreen;
        Id = Guid.NewGuid().ToString();
        Name = Id;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsEnabled { get; set; }
    public Color BoxColor { get; set; }

    public Rect Bounds => new Rect(X, Y, Width, Height);
    public BaseNode Node { get; set; }
}
