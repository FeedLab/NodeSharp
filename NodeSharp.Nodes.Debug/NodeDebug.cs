using System.Text.Json;
using System.Text.Json.Serialization;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.Debug;

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
        List<Output> outputs,
        List<Input> inputs,
        JsonElement nodeElement)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, outputs, inputs)
    {
        OutputJsonMessage = "";
    }

    public override Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        try
        {
            EnterNode(this);

            base.RunFromInput(parentNode, parametersJsonString);
            OutputJsonMessage = parametersJsonString;

            System.Diagnostics.Debug.WriteLine($"{Name}: {parametersJsonString}");

            ExitNodeMessage(this, "Output", parametersJsonString, Name);

            return Task.FromResult(parametersJsonString);
        }
        finally
        {
            LeaveNode(this);
        }
    }

    [JsonIgnore] public string OutputJsonMessage { get; set; }
}