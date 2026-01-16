using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Random.ViewModel;

namespace NodeSharp.Nodes.Random;

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
        try
        {
            if (!nodeElement.TryGetProperty("RandomData", out var randomProp) ||
                randomProp.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("RandomData object not found or invalid");
            }

            RandomData = new RandomDataPayload(randomProp);
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(this, nameof(RandomData), e);
        }
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {
        var stopWatch = EnterNode(this);

        try
        {
            var fromInput = await base.RunFromInput(parentNode, inputJsonString);

            var randomNumber = 0;
            if (RandomData.Source.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
            {
                randomNumber = RandomNumberFromFixedData();
            }
            else if (RandomData.Source.Equals("FromInput", StringComparison.OrdinalIgnoreCase))
            {
                randomNumber = RandomNumberFromInputData(inputJsonString);
            }
            else
            {
                throw new InvalidOperationException($"NodeRandomNumber '{Name}' has invalid Source: {RandomData.Source}.");
            }
            string updatedJsonString;
            try
            {
                var parsed = string.IsNullOrWhiteSpace(inputJsonString)
                    ? null
                    : JsonNode.Parse(inputJsonString);
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
            
            MainThread.BeginInvokeOnMainThread(() => { BoxNodeStatus.Message = $"Rnd: {randomNumber}"; });
            
            var jsonNode = JsonNode.Parse(updatedJsonString) ?? "";

            OutputMessage = await SendToConnectedChildrenAsync(jsonNode);

            return fromInput;
        }
        finally
        {
            LeaveNode(this, stopWatch);
        }
    }
    private int RandomNumberFromFixedData()
    {
        if (RandomData.Max < RandomData.Min)
        {
            throw new InvalidOperationException(
                $"NodeRandomNumber '{Name}' has invalid range: Min ({RandomData.Min}) must be <= Max ({RandomData.Max}).");
        }
        return System.Random.Shared.Next(RandomData.Min, RandomData.Max + 1);
    }
    private int RandomNumberFromInputData(string parametersJsonString)
    {
        var parsed = string.IsNullOrWhiteSpace(parametersJsonString)
            ? null
            : JsonNode.Parse(parametersJsonString);

        int randomMin, randomMax;

        if (parsed is JsonObject obj)
        {
            randomMin = obj["RandomMin"]?.GetValue<int>() ?? throw new InvalidOperationException("Missing 'RandomMin' in input object.");
            randomMax = obj["RandomMax"]?.GetValue<int>() ?? throw new InvalidOperationException("Missing 'RandomMax' in input object.");
        }
        else if (parsed is JsonArray parameters)
        {
            int GetNumber(string name)
            {
                var param = parameters
                                .OfType<JsonObject>()
                                .FirstOrDefault(p => p.ContainsKey(name))
                            ?? throw new InvalidOperationException($"Missing parameter '{name}' in input array.");
                return param[name]!.GetValue<int>();
            }
            randomMin = GetNumber("RandomMin");
            randomMax = GetNumber("RandomMax");
        }
        else
        {
            throw new InvalidOperationException("Input data is not a valid JSON Object or Array.");
        }

        if (randomMax < randomMin)
        {
            throw new InvalidOperationException(
                $"NodeRandomNumber '{Name}' has invalid range: Min ({randomMin}) must be <= Max ({randomMax}).");
        }
        return System.Random.Shared.Next(randomMin, randomMax + 1);
    }
    
    public override async Task DisplayNodeConfigurationPopup()
    {
        var configurationPopupViewModel = AppService.GetService<RandomConfigurePopupViewModel>();

        if (configurationPopupViewModel is null)
        {
            Debug.WriteLine("RandomConfigurePopupViewModel is null. That means NO configuration popup will be shown. This should be not happen. Remove DisplayNodeConfigurationPopup for the node");
            return;
        }
        
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(NodeRandomNumber)] = this
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        await PopupService.ShowPopupAsync<RandomConfigurePopupViewModel>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);
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