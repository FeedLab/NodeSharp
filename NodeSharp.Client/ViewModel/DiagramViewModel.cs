using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    public void MoveNodeToFront(BoxNode node)
    {
        if (Nodes.Contains(node) && Nodes.Last() != node)
        {
            Nodes.Remove(node);
            Nodes.Add(node);
        }
    }
}

public class BoxNode : INotifyPropertyChanged
{
    private double _x;
    private double _y;
    private double _width;
    private double _height;

    public BoxNode()
    {
        BoxColor = Colors.ForestGreen;
        Id = Guid.NewGuid().ToString();
        Name = Id;
    }

    public string Id { get; set; }
    public string Name { get; set; }

    public double X
    {
        get => _x;
        set
        {
            if (_x != value)
            {
                _x = value;
                OnPropertyChanged();
            }
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            if (_y != value)
            {
                _y = value;
                OnPropertyChanged();
            }
        }
    }

    public double Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEnabled { get; set; }
    public Color BoxColor { get; set; }

    public Rect Bounds => new Rect(X, Y, Width, Height);
    public BaseNode Node { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
