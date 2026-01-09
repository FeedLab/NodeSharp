namespace NodeSharp.Nodes.Common.Model;

public class Storage
{
    private readonly NodeInformationDictionary nodeInformationDictionary = new NodeInformationDictionary();
    
    public NodeInformationDictionary GetNodeInformation()
    {
        return nodeInformationDictionary;
    }
}