using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.Component;
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
        if (e.NewItems is [BaseNode newNode])
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var boxNode = new BoxNode(newNode);
                    BoxNodes.Add(boxNode);
                    
                    WeakReferenceMessenger.Default.Send(new NodeActionEvent
                        { ActionEventType = NodeActionEventType.Add });
                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var lookupBoxNode = BoxNodes.First(x => x.Node.Id == newNode.Id);
                    BoxNodes.Remove(lookupBoxNode);
                    
                    WeakReferenceMessenger.Default.Send(new NodeActionEvent
                        { ActionEventType = NodeActionEventType.Delete });

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