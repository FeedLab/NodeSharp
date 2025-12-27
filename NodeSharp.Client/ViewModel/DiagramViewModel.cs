using System.Collections.ObjectModel;
using System.Text.Json;
using NodeSharp.Client.Component;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client.ViewModel;

public class DiagramViewModel(NodeIo nodeIo)
{
    private const string BaseFilePath = ".";
    public ObservableCollection<BoxNode> BoxNodes { get; } = new();
    
    public async Task Init(StreamReader reader, string filePath)
    {
        await nodeIo.LoadFromFileAsync(reader, filePath);

        foreach (var node in nodeIo.Nodes)
        {
            var boxNode = new BoxNode(node);

            BoxNodes.Add(boxNode);
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