using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConsoleApp1.Extension;

namespace ConsoleApp1;

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

    protected override async Task RunFromInput(BaseNode parentNode, string parametersJsonString)
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
            
            // if (parsed is JsonArray parameters)
            // {
            //     var nodeDelay = parsed.GetByPath(FromInputPath);
            //     if (nodeDelay is null)
            //     {
            //         throw new InvalidOperationException($"NodeDelay '{Name}' could not find parameter '{FromInputPath}'");
            //     }
            //     
            //     int GetNumber(string name)
            //     {
            //         var param = parameters
            //                         .OfType<JsonObject>()
            //                         .FirstOrDefault(p => p.ContainsKey(name))
            //                     ?? throw new InvalidOperationException($"Missing parameter '{name}'");
            //
            //         return param[name]!.GetValue<int>();
            //     }
            //
            //     var delayValue = GetNumber("Delay");
            //
            //     delayMilliseconds = Type.ToLowerInvariant() switch
            //     {
            //         "milliseconds" => delayValue,
            //         "seconds" => delayValue * 1000,
            //         "minutes" => delayValue * 60 * 1000,
            //         _ => throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Type: {Type}")
            //     };
            // }
        }
        else
        {
            throw new InvalidOperationException($"NodeDelay '{Name}' has invalid Source: {Source}.");
        }

        Debug.WriteLine($"NodeDelay '{Name}' delaying for {delayMilliseconds}ms");
        await Task.Delay(delayMilliseconds);

        await SendToChildren(parametersJsonString);
    }

    private void ValidateNodeId()
    {
        Debug.WriteLine($"Validating NodeId: {Id}");

        if (!Guid.TryParse(Id, out _))
        {
            Debug.WriteLine($"NodeId '{Id}' is not a valid GUID - throwing exception");
            throw new InvalidOperationException($"NodeId '{Id}' is not a valid GUID.");
        }

        Debug.WriteLine($"NodeId is valid");
    }

    private void ValidateInputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Inputs.Length} input connections");
        var errors = new StringBuilder();

        foreach (var input in Inputs)
        {
            Debug.WriteLine($"Validating input: {input.Name}");

            foreach (var nodeId in input.ConnectsToParentNodeId)
            {
                Debug.WriteLine($"Checking parent node connection: {nodeId}");

                if (!Guid.TryParse(nodeId, out _))
                {
                    Debug.WriteLine($"Invalid GUID: {nodeId}");
                    errors.Append($"Input ConnectsToParentNodeId '{nodeId}' is not a valid GUID. ");
                }
                else if (baseNodeList.All(n => n.Id != nodeId))
                {
                    Debug.WriteLine($"Parent node not found: {nodeId}");
                    errors.Append($"Input '{input.Name}' connects to non-existing parent node '{nodeId}'. ");
                }
                else
                {
                    Debug.WriteLine($"Parent node connection valid: {nodeId}");
                }
            }
        }

        if (errors.Length > 0)
        {
            Debug.WriteLine($"Validation errors found: {errors}");
            throw new InvalidOperationException(errors.ToString().Trim());
        }
    }

    private void ValidateOutputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Outputs.Length} output connections");
        var errors = new StringBuilder();

        foreach (var output in Outputs)
        {
            Debug.WriteLine($"Validating output: {output.Name}");

            foreach (var nodeId in output.ConnectsToNodeId)
            {
                Debug.WriteLine($"Checking node connection: {nodeId}");

                if (!Guid.TryParse(nodeId, out _))
                {
                    Debug.WriteLine($"Invalid GUID: {nodeId}");
                    errors.Append($"Output ConnectsToNodeId '{nodeId}' is not a valid GUID. ");
                }
                else if (baseNodeList.All(n => n.Id != nodeId))
                {
                    Debug.WriteLine($"Node not found: {nodeId}");
                    errors.Append($"Output '{output.Name}' connects to non-existing node '{nodeId}'. ");
                }
                else
                {
                    Debug.WriteLine($"Node connection valid: {nodeId}");
                }
            }
        }

        if (errors.Length > 0)
        {
            Debug.WriteLine($"Validation errors found: {errors}");
            throw new InvalidOperationException(errors.ToString().Trim());
        }
    }
}