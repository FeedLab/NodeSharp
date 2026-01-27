using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Collection;
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
        Storage storage,
        Color backgroundColor)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage, backgroundColor)
    {
        const double height = 70;
        const double width = 220;
        const double bodyHeight = 12;

        BoxDimension = new Rect(0, 0, width, height);

        Inputs.Clear();
        Outputs.Clear();

        Inputs.Add(new Input(Guid.CreateVersion7(), "Input", [], new Point(0, (height - bodyHeight) / 2)));
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
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {
        var stopwatch = EnterNode(this);

        try
        {
            var fromInput = await base.RunFromInput(parentNode, inputJsonString);

            if (fromInput is null)
            {
                throw new InvalidOperationException($"NodeDebug '{Name}' has no input data.");
            }

            OutputMessage = inputJsonString;

            System.Diagnostics.Debug.WriteLine($"{Name}: {inputJsonString}");

            ExitNodeMessage(
                this,
                "Output",
                fromInput.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                Name);

            return fromInput;
        }
        finally
        {
            LeaveNode(this, stopwatch);
        }
    }
}