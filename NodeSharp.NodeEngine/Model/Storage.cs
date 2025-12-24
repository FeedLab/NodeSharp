namespace NodeSharp.NodeEngine.Model;

public class Storage
{
    private readonly NodeInformationDictionary nodeInformationDictionary = new NodeInformationDictionary();
    
    public NodeInformationDictionary GetNodeInformation()
    {
        return nodeInformationDictionary;
    }
}