using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.NodeEngine.Node;

public class NodeRandomNumber : BaseNode
{
    [JsonInclude] private RandomDataPayload RandomData { get; set; }

    public NodeRandomNumber(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition,
        int yPosition,
        Storage storage,
        RandomDataPayload randomData)
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
        RandomData = randomData;
    }


    public NodeRandomNumber(
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
        if (!nodeElement.TryGetProperty("RandomData", out var randomProp) ||
            randomProp.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("RandomData object not found or invalid");
        }

        RandomData = new RandomDataPayload(randomProp);
    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        await base.RunFromInput(parentNode, parametersJsonString);

        var random = new Random();
        var randomNumber = 0;

        if (RandomData.Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
        {
            randomNumber = RandomNumberFromFixedData(random);
        }
        else if (RandomData.Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
        {
            randomNumber = RandomNumberFromInputData(parametersJsonString, randomNumber, random);
        }
        else
        {
            throw new InvalidOperationException($"NodeRandomNumber '{Name}' has invalid Source: {RandomData.Source}.");
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
        if (RandomData.Max < RandomData.Min)
        {
            throw new InvalidOperationException(
                $"NodeRandomNumber '{Name}' has invalid range: Min ({RandomData.Min}) must be <= Max ({RandomData.Max}).");
        }

        randomNumber = random.Next(RandomData.Min, RandomData.Max + 1);
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
}

public class RandomDataPayload
{
    public string Source { get; }

    public int Min { get; }

    public int Max { get; }

    public RandomDataPayload(JsonElement element)
    {
        Source = !element.TryGetProperty("Source", out var sourceProp) || sourceProp.ValueKind != JsonValueKind.String
            ? throw new InvalidOperationException("Source value not found or invalid")
            : sourceProp.GetString() ?? "Fixed";
        Min = !element.TryGetProperty("Min", out var minProp) || minProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Min value not found or invalid")
            : minProp.GetInt32();
        Max = !element.TryGetProperty("Max", out var maxProp) || maxProp.ValueKind != JsonValueKind.Number
            ? throw new InvalidOperationException("Max value not found or invalid")
            : maxProp.GetInt32();
    }

    public RandomDataPayload(string source = "Fixed", int min = 0, int max = 100)
    {
        Source = source;
        Min = min;
        Max = max;
    }
}