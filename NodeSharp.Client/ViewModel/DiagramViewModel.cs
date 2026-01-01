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
        if (e.NewItems is [BaseNode newNode])
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var boxNode = new BoxNode(newNode);
                    BoxNodes.Add(boxNode);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var lookupBoxNode = BoxNodes.First(x => x.Node.Id == newNode.Id);
                    BoxNodes.Remove(lookupBoxNode);
                    break;
            }
        }
        else
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                BoxNodes.Clear();
            }
            else
            {
                throw new InvalidOperationException("New items must be a single BaseNode.");
            }
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