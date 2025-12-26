using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using NodeSharp.NodeEngine.Extension;

namespace NodeSharp.NodeEngine.Node;

public class NodeDelay : BaseNode
{
    private string PathToDelayNode { get; }
    private string Source { get; }
    private string Type { get; }
    private int Value { get; }

    public NodeDelay(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
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
            outputs,
            inputs
)
    {
        if (!nodeElement.TryGetProperty("Delay", out var delayProp) || delayProp.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Delay object not found or invalid");
        }

        PathToDelayNode = !delayProp.TryGetProperty("PathToDelayNode", out var pathToDelayNodeProp) || pathToDelayNodeProp.ValueKind != JsonValueKind.String 
            ? throw new InvalidOperationException("Delay.PathToDelayNode value not found or invalid")
            : pathToDelayNodeProp.GetString() ?? throw new InvalidOperationException("Delay.PathToDelayNode is null");
        
        Source = !delayProp.TryGetProperty("Source", out var sourceProp) || sourceProp.ValueKind != JsonValueKind.String 
            ? throw new InvalidOperationException("Delay.Source value not found or invalid")
            : sourceProp.GetString() ?? throw new InvalidOperationException("Delay.Source is null");

        Type = !delayProp.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Delay.Type value not found or invalid") 
            : typeProp.GetString() ?? throw new InvalidOperationException("Delay.Type is null");

        Value = !delayProp.TryGetProperty("Value", out var valueProp) || valueProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Delay.Value value not found or invalid")
            : valueProp.GetInt32();
    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        await base.RunFromInput(parentNode, parametersJsonString);

        var delayMilliseconds = 0;

        if (Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
        {
            delayMilliseconds = Type.ToLowerInvariant() switch
            {
                "milliseconds" => Value,
                "seconds" => Value * 1000,
                "minutes" => Value * 60 * 1000,
                _ => throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Type: {Type}")
            };
        }
        else if (Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
        {
            var root = string.IsNullOrWhiteSpace(parametersJsonString)
                ? null
                : JsonNode.Parse(parametersJsonString);

            
            var delayNode = root.GetByPath(PathToDelayNode) as JsonObject;

            var valueNode = delayNode?[nameof(Value)];
            var typeNode = delayNode?[nameof(Type)];

            var delayValue = valueNode?.GetValue<int>()
                             ?? throw new InvalidOperationException($"NodeDelay '{Name}' missing or invalid 'Value' at '{PathToDelayNode}.Value'");

            var delayType = typeNode?.GetValue<string>()
                            ?? throw new InvalidOperationException($"NodeDelay '{Name}' missing or invalid 'Type' at '{PathToDelayNode}.Type'");

            delayMilliseconds = delayType.ToLowerInvariant() switch
            {
                "milliseconds" => delayValue,
                "seconds" => delayValue * 1000,
                "minutes" => delayValue * 60 * 1000,
                _ => throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Type: {Type}")
            };
        }
        else
        {
            throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Source: {Source}.");
        }

        Debug.WriteLine($"NodeDelay '{Name}' delaying for {delayMilliseconds}ms");
        await Task.Delay(delayMilliseconds);

        await SendToConnectedChildrenAsync(parametersJsonString);
        
        return await Task.FromResult(parametersJsonString);
    }
    
}