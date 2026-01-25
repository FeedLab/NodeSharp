using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common;

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
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems is [BaseNode newNode]:
            {
                var boxNode = new BoxNode(newNode);
                BoxNodes.Add(boxNode);

                WeakReferenceMessenger.Default.Send(new NodeActionEvent
                    { ActionEventType = NodeActionEventType.Add });
                break;
            }
            case NotifyCollectionChangedAction.Remove when e.OldItems is [BaseNode oldNode]:
            {
                var lookupBoxNode = BoxNodes.First(x => x.Node.Id == oldNode.Id);
                BoxNodes.Remove(lookupBoxNode);

                WeakReferenceMessenger.Default.Send(new NodeActionEvent
                    { ActionEventType = NodeActionEventType.Delete });
                break;
            }
            case NotifyCollectionChangedAction.Reset:
                BoxNodes.Clear();
                break;
            default:
                throw new InvalidOperationException("Collection change must contain a single BaseNode.");
        }
    }
    
    public void PrepareForSave()
    {
        foreach (var node in BoxNodes)
        {
            node.PrepareForSave();
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
