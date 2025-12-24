using System.Diagnostics;
using System.Text.Json;

namespace NodeSharp.NodeEngine.Node;

public class NodeDebug : BaseNode
{
    public NodeDebug(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        Output[] outputs,
        Input[] inputs,
        JsonElement nodeElement)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs)
    {
        OutputJsonMessage = "";
    }

    public override Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        base.RunFromInput(parentNode, parametersJsonString);
        OutputJsonMessage = parametersJsonString;
        
        Debug.WriteLine($"{Name}: {parametersJsonString}");
        
        return Task.FromResult(parametersJsonString);
    }

    public string OutputJsonMessage { get; set; }
}
