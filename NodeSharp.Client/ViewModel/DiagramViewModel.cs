using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using NodeSharp.Client.Component;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.ViewModel;

public class DiagramViewModel
{
    private readonly NodeIo nodeIo;

    public DiagramViewModel(NodeIo nodeIo)
    {
        this.nodeIo = nodeIo;
        nodeIo.Nodes.CollectionChanged += OnNodesCollectionChanged;
    }

    private const string BaseFilePath = ".";
    public ObservableCollection<BoxNode> BoxNodes { get; } = new();

    public async Task Init(StreamReader reader, string filePath)
    {

        await nodeIo.LoadFromFileAsync(reader, filePath);

        // foreach (var node in nodeIo.Nodes)
        // {
        //     var boxNode = new BoxNode(node);
        //
        //     BoxNodes.Add(boxNode);
        // }
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                if (e.NewItems is { Count: 1 })
                {
                    var newNode = (BaseNode)e.NewItems[0];
                    var boxNode = new BoxNode(newNode);
                    BoxNodes.Add(boxNode);
                }
                else
                {
                    throw new Exception("Only one node can be added at a time.");
                }

                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                // Handle nodes removed
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                // Handle list cleared
                break;
        }
    }

    public void MoveNodeToFront(BoxNode node)
    {
        if (BoxNodes.Contains(node) && BoxNodes.Last() != node)
        {
            BoxNodes.Remove(node);
            BoxNodes.Add(node);
        }
    }

    public void Clear()
    {
        BoxNodes.Clear();
    }
}