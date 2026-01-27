using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Collection;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.Delay;

public class NodeDelay : BaseNode
{
    [JsonInclude] private DelayPayload Delay { get; set; }

    public NodeDelay(
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
        : base(
            nodes,
            id,
            typeId,
            name,
            isEnabled,
            activateOnStart,
            xPosition,
            yPosition,
            storage,
            backgroundColor
        )
    {
        Delay = new DelayPayload();

        var statusBodyHeight = InitializeDimensions();

        Inputs.Clear();
        Outputs.Clear();

        Inputs.Add(new Input(Guid.CreateVersion7(), "Input", [],
            new Point(0, (BoxDimension.Height - statusBodyHeight) / 2)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Output", [],
            new Point(0, (BoxDimension.Height - statusBodyHeight) / 2)));
    }

    private double InitializeDimensions()
    {
        const double height = 70;
        const double width = 280;
        const double statusBodyHeight = 12;

        BoxDimension = new Rect(0, 0, width, height);

        return statusBodyHeight;
    }

    public NodeDelay(
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
        : base(
            nodes,
            id,
            typeId,
            name,
            isEnabled,
            activateOnStart,
            xPosition,
            yPosition,
            outputs,
            inputs
        )
    {
        InitializeDimensions();
        
        
        if (!nodeElement.TryGetProperty("Delay", out var delayProp) || delayProp.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Delay object not found or invalid");
        }

        try
        {
            Delay = new DelayPayload(delayProp);
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(this, nameof(Delay), e);
        }
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {
        var stopwatch = EnterNode(this);

        try
        {
            var fromInput = await base.RunFromInput(parentNode, inputJsonString);

            var delayMilliseconds = 0;

            if (Delay.Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
            {
                delayMilliseconds = Delay.Type.ToLowerInvariant() switch
                {
                    "milliseconds" => Delay.Value,
                    "seconds" => Delay.Value * 1000,
                    "minutes" => Delay.Value * 60 * 1000,
                    _ => throw new InvalidOperationException(
                        $"NodeDelay '{this.GetType().Name}' has invalid Type: {Delay.Type}")
                };
            }
            else if (Delay.Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
            {
                var root = string.IsNullOrWhiteSpace(inputJsonString)
                    ? null
                    : JsonNode.Parse(inputJsonString);


                var delayNode = root.GetByPath(Delay.PathToDelayNode) as JsonObject;

                if (delayNode == null)
                {
                    throw new InvalidOperationException(
                        $"NodeDelay at path '{Delay.PathToDelayNode}' not found or is not an object.");
                }

                int delayValue;
                try
                {
                    delayValue = delayNode[nameof(Delay.Value)]?.GetValue<int>()
                                 ?? throw new InvalidOperationException("Value node is missing.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"NodeDelay missing or invalid 'Value' at '{Delay.PathToDelayNode}.Value'", ex);
                }

                string delayType;
                try
                {
                    // Using a string literal or ensuring the correct property name is used
                    delayType = delayNode["Type"]?.GetValue<string>()
                                ?? throw new InvalidOperationException("Type node is missing.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"NodeDelay missing or invalid 'Type' at '{Delay.PathToDelayNode}.Type'", ex);
                }

                delayMilliseconds = delayType.ConvertTimeToMilliseconds(delayValue);
            }
            else
            {
                throw new InvalidOperationException(
                    $"NodeDelay '{this.GetType().Name}' has invalid Source: {Delay.Source}.");
            }

            if (delayMilliseconds > 0)
            {
                Debug.WriteLine($"NodeDelay '{this.GetType().Name}' delaying for {delayMilliseconds}ms");

                await PeriodicExecutor.DelayedPeriodicExecution(
                    delay: TimeSpan.FromSeconds(delayMilliseconds / 1000),
                    interval: TimeSpan.FromMilliseconds(500),
                    action: (percentComplete) => { BoxNodeStatus.Value = (decimal)percentComplete; },
                    cancellationToken: Cts.Token);
            }

            await SendToConnectedChildrenAsync(inputJsonString);

            return fromInput;
        }
        finally
        {
            LeaveNode(this, stopwatch);
        }
    }
}

public class DelayPayload
{
    public DelayPayload(JsonElement element)
    {
        PathToDelayNode = !element.TryGetProperty("PathToDelayNode", out var pathToDelayNodeProp) ||
                          pathToDelayNodeProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Delay.PathToDelayNode value not found or invalid")
            : pathToDelayNodeProp.GetString() ??
              throw new InvalidOperationException("Delay.PathToDelayNode is null");

        Source = !element.TryGetProperty("Source", out var sourceProp) ||
                 sourceProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Delay.Source value not found or invalid")
            : sourceProp.GetString() ?? throw new InvalidOperationException("Delay.Source is null");

        Type = !element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Delay.Type value not found or invalid")
            : typeProp.GetString() ?? throw new InvalidOperationException("Delay.Type is null");

        Value = !element.TryGetProperty("Value", out var valueProp) || valueProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Delay.Value value not found or invalid")
            : valueProp.GetInt32();
    }

    public DelayPayload(string pathToDelayNode = "payload", string source = "Fixed", string type = "seconds",
        int value = 5)
    {
        PathToDelayNode = pathToDelayNode;
        Source = source;
        Type = type;
        Value = value;
    }

    public string PathToDelayNode { get; }

    public string Source { get; }

    public string Type { get; }

    public int Value { get; }
}