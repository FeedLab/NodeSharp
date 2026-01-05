namespace NodeSharp.NodeEngine.Model;

public class NodeInformationDictionary : Dictionary<string, NodeInformation>
{
    public NodeInformationDictionary()
    {
        var informationText =
            "Want me to refactor this to use MVVM entirely (no code-behind), with SelectedItem bound to your ViewModel and a CollectionChanged hook to keep highlights in sync";
        
        Add("Inject", new NodeInformation("Inject", true, true, informationText,"\ue713", 0, 1));
        Add("Debug", new NodeInformation("Debug", false, true, informationText,"\ue714", 1, 0));
        Add("Delay", new NodeInformation("Delay", false, true, informationText,"\ue715", 1, 1));
        Add("RandomNumber", new NodeInformation("RandomNumber", false, true, informationText,"\ue716", 1, 1));
        Add("Function", new NodeInformation("Function", false, true, informationText,"\ue718", 1, 1));
    }
}