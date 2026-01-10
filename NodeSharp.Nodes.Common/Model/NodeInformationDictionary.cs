using System;
using System.Collections.Generic;
using System.Reflection;
using NodeSharp.Nodes.Common.Helper;

namespace NodeSharp.Nodes.Common.Model;

public class NodeInformationDictionary : Dictionary<string, INodeInformation>
{
    public NodeInformationDictionary()
    {
        var types = AssemblyHelper.FindImplementations<INodeInformation>(AppContext.BaseDirectory);

        foreach (var type in types)
        {
            if (type.FullName == "NodeSharp.Nodes.Common.Model.NodeInformation")
            {
                continue;
            }
            
            var instance = (INodeInformation)Activator.CreateInstance(type)!;

            Add(instance.TypeId, instance);
        }

        var informationText =
            "Want me to refactor this to use MVVM entirely (no code-behind), with SelectedItem bound to your ViewModel and a CollectionChanged hook to keep highlights in sync";
        
        // Add("Inject", new NodeInformation("Inject", true, true, informationText,"\ue713", 0, 1));
        // Add("Debug", new NodeInformation("Debug", "NodeDebug",false, true, informationText,"\ue714", 1, 0));
        // Add("Delay", new NodeInformation("Delay", "NodeDelay",false, true, informationText,"\ue715", 1, 1));
      //  Add("RandomNumber", new NodeInformation("RandomNumber", "NodeRandomNumber",false, true, informationText,"\ue716", 1, 1));
        // Add("Function", new NodeInformation("Function", "NodeFunction",false, true, informationText,"\ue718", 1, 1));
    }
}