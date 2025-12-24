using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NodeSharp.NodeEngine.Node;

public class NodeRandomNumber : BaseNode
{
    private string Source { get; }
    private int Min { get; }
    private int Max { get; }

    public NodeRandomNumber(
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
        Source = !nodeElement.TryGetProperty("Source", out var sourceProp) || sourceProp.ValueKind != JsonValueKind.String 
            ? throw new InvalidOperationException("Source value not found or invalid")
            : sourceProp.GetString() ?? "Fixed";
        Min = !nodeElement.TryGetProperty("Min", out var minProp) || minProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Min value not found or invalid") 
            : minProp.GetInt32();
        Max = !nodeElement.TryGetProperty("Max", out var maxProp) || maxProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Max value not found or invalid")
            : maxProp.GetInt32();
    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        await base.RunFromInput(parentNode, parametersJsonString);

        var random = new Random();
        var randomNumber = 0;

        if (Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
        {
            randomNumber = RandomNumberFromFixedData(random);
        }
        else if (Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
        {
            randomNumber = RandomNumberFromInputData(parametersJsonString, randomNumber, random);
        }
        else
        {
            throw new InvalidOperationException($"NodeRandomNumber '{Name}' has invalid Source: {Source}.");
        }

        string updatedJsonString;

        try
        {
            var parsed = string.IsNullOrWhiteSpace(parametersJsonString)
                ? null
                : JsonNode.Parse(parametersJsonString);

            if (parsed is JsonObject obj)
            {
                obj["RandomNumber"] = randomNumber;
                updatedJsonString = obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
            else if (parsed is JsonArray arr)
            {
                arr.Add(new JsonObject { ["RandomNumber"] = randomNumber });
                updatedJsonString = arr.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                var obj2 = new JsonObject { ["RandomNumber"] = randomNumber };
                updatedJsonString = obj2.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"NodeRandomNumber '{Name}' received invalid JSON from parent '{parentNode.Name}'.", ex);
        }

        await SendToConnectedChildrenAsync(updatedJsonString);
        
        return await Task.FromResult(updatedJsonString);
    }

    private int RandomNumberFromFixedData(Random random)
    {
        int randomNumber;
        if (Max < Min)
        {
            throw new InvalidOperationException(
                $"NodeRandomNumber '{Name}' has invalid range: Min ({Min}) must be <= Max ({Max}).");
        }

        randomNumber = random.Next(Min, Max + 1);
        return randomNumber;
    }

    private int RandomNumberFromInputData(string parametersJsonString, int randomNumber, Random random)
    {
        var parsed = string.IsNullOrWhiteSpace(parametersJsonString)
            ? null
            : JsonNode.Parse(parametersJsonString);

        if (parsed is JsonArray parameters)
        {
            int GetNumber(string name)
            {
                var param = parameters
                                .OfType<JsonObject>()
                                .FirstOrDefault(p => p.ContainsKey(name))
                            ?? throw new InvalidOperationException($"Missing parameter '{name}'");

                return int.Parse(param[name]!.ToString());
            }

            var randomMin = GetNumber("RandomMin");
            var randomMax = GetNumber("RandomMax");


            if (randomMax < randomMin)
            {
                throw new InvalidOperationException(
                    $"NodeRandomNumber '{Name}' has invalid range: Min ({randomMin}) must be <= Max ({randomMax}).");
            }

            randomNumber = random.Next(randomMin, randomMax + 1);
        }

        return randomNumber;
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

    


}