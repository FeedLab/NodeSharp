using System.Collections.ObjectModel;
using Facet.Extensions;
using Microsoft.Extensions.Logging;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client.ViewModel;

public class NodeToolListComponentModel(ILogger<NodeToolListComponentModel> logger, Storage storage)
{
    private readonly ILogger<NodeToolListComponentModel> logger = logger;
    private readonly ObservableCollection<NodeInformationModel> nodes = [];

    public ObservableCollection<NodeInformationModel> Nodes => nodes;

    public void Init()
    {
        foreach (var nodeInformation in storage.GetNodeInformation())
        {
            // Convert each NodeInformation into a NodeInformationModel
            var nodeInformationModel = nodeInformation.Value.ToFacet<NodeInformation, NodeInformationModel>();
            
            nodes.Add(nodeInformationModel);
        }
    }
}