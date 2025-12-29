using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Model;

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
        int xPosition, 
        int yPosition,
        Storage storage)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage)
    {
        OutputJsonMessage = "";
    }
    
    public NodeDebug(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition, 
        int yPosition,
        Output[] outputs,
        Input[] inputs,
        JsonElement nodeElement)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, outputs, inputs)
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

    [JsonIgnore]
    public string OutputJsonMessage { get; set; }
}
