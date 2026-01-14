using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Client.ViewModel;

public class NodeToolListModel(ILogger<NodeToolListModel> logger, Storage storage)
{
    private readonly ILogger<NodeToolListModel> logger = logger;
    private ObservableCollection<INodeInformation>? nodes;

    public ObservableCollection<INodeInformation> Nodes
    {
        get
        {
            nodes ??= [];
            
            Init();
            
            return nodes;
        }
    }

    private void Init()
    {
        foreach (var nodeInformation in storage.GetNodeInformation())
        {
             nodes?.Add(nodeInformation.Value.NodeInformation);
        }
    }
}