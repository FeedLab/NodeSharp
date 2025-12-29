using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Extension;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.NodeEngine.Node;

public class NodeDelay : BaseNode
{
    [JsonInclude]
    private DelayPayload Delay { get; set; }

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
        DelayPayload delay
            )
        : base(
            nodes,
            id,
            typeId,
            name,
            isEnabled,
            activateOnStart,
            xPosition,
            yPosition,
            storage
        )
    {
        Delay = delay;
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
        Output[] outputs,
        Input[] inputs,
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
        if (!nodeElement.TryGetProperty("Delay", out var delayProp) || delayProp.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Delay object not found or invalid");
        }

        Delay = new DelayPayload(delayProp);
        

    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        await base.RunFromInput(parentNode, parametersJsonString);

        var delayMilliseconds = 0;

        if (Delay.Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
        {
            delayMilliseconds = Delay.Type.ToLowerInvariant() switch
            {
                "milliseconds" => Delay.Value,
                "seconds" => Delay.Value * 1000,
                "minutes" => Delay.Value * 60 * 1000,
                _ => throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Type: {Delay.Type}")
            };
        }
        else if (Delay.Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
        {
            var root = string.IsNullOrWhiteSpace(parametersJsonString)
                ? null
                : JsonNode.Parse(parametersJsonString);

            
            var delayNode = root.GetByPath(Delay.PathToDelayNode) as JsonObject;

            var valueNode = delayNode?[nameof(Delay.Value)];
            var typeNode = delayNode?[nameof(Type)];

            var delayValue = valueNode?.GetValue<int>()
                             ?? throw new InvalidOperationException($"NodeDelay '{Name}' missing or invalid 'Value' at '{Delay.PathToDelayNode}.Value'");

            var delayType = typeNode?.GetValue<string>()
                            ?? throw new InvalidOperationException($"NodeDelay '{Name}' missing or invalid 'Type' at '{Delay.PathToDelayNode}.Type'");

            delayMilliseconds = delayType.ToLowerInvariant() switch
            {
                "milliseconds" => delayValue,
                "seconds" => delayValue * 1000,
                "minutes" => delayValue * 60 * 1000,
                _ => throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Type: {Delay.Type}")
            };
        }
        else
        {
            throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Source: {Delay.Source}.");
        }

        Debug.WriteLine($"NodeDelay '{Name}' delaying for {delayMilliseconds}ms");
        await Task.Delay(delayMilliseconds);

        await SendToConnectedChildrenAsync(parametersJsonString);
        
        return await Task.FromResult(parametersJsonString);
    }
    
}

public class DelayPayload
{
    public DelayPayload(JsonElement element)
    {
        PathToDelayNode = !element.TryGetProperty("PathToDelayNode", out var pathToDelayNodeProp) || pathToDelayNodeProp.ValueKind != JsonValueKind.String 
            ? throw new InvalidOperationException("Delay.PathToDelayNode value not found or invalid")
            : pathToDelayNodeProp.GetString() ?? throw new InvalidOperationException("Delay.PathToDelayNode is null");
        
        Source = !element.TryGetProperty("Source", out var sourceProp) || sourceProp.ValueKind != JsonValueKind.String 
            ? throw new InvalidOperationException("Delay.Source value not found or invalid")
            : sourceProp.GetString() ?? throw new InvalidOperationException("Delay.Source is null");

        Type = !element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Delay.Type value not found or invalid") 
            : typeProp.GetString() ?? throw new InvalidOperationException("Delay.Type is null");

        Value = !element.TryGetProperty("Value", out var valueProp) || valueProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Delay.Value value not found or invalid")
            : valueProp.GetInt32();
    }

    public DelayPayload(string pathToDelayNode = "payload", string source = "Fixed", string type = "seconds", int value = 5)
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