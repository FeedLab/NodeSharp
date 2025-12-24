namespace NodeSharp.NodeEngine.Model;

public class NodeInformationDictionary : Dictionary<string, NodeInformation>
{
    public NodeInformationDictionary()
    {
        Add("Debug", new NodeInformation("Debug", true, true, "Debug"));
    }
}
