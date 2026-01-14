using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.KS0212.ViewModel;

namespace NodeSharp.Nodes.KS0212;

public class NodeKs0212 : BaseNode
{
    public RelaySettings RelaySettings { get; }

    public NodeKs0212(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition,
        int yPosition,
        Storage storage)
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
        RelaySettings = new RelaySettings();
    }


    public NodeKs0212(
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
            RelaySettings = new RelaySettings(nodeElement);
        }
        catch (Exception e)
        {
            throw new NodeParseException(this, nameof(RelaySettings), e);
        }
    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        try
        {
            EnterNode(this);

            await base.RunFromInput(parentNode, parametersJsonString);

            const string errorMessage = "An error occurred. Make sure 'msg.payload' is present in the input JSON.";

            var jsonElement = JsonDocument.Parse(parametersJsonString).RootElement;
            JsonElement payloadElement;
            if (jsonElement.TryGetProperty("payload", out var payload))
                payloadElement = payload;
            else
            {
                var jsonMessage = JsonErrorMessage(parametersJsonString, errorMessage);

                await SendToConnectedChildrenAsync(jsonMessage);

                return OutputMessage;
            }

            var incomingSettings = new RelaySettings(payloadElement);
            RelaySettings.One = incomingSettings.One;
            RelaySettings.Two = incomingSettings.Two;
            RelaySettings.Three = incomingSettings.Three;
            RelaySettings.Four = incomingSettings.Four;

            var updatedJsonString = parametersJsonString;

            var jsonNode = JsonNode.Parse(updatedJsonString) ?? "";

            await SendToConnectedChildrenAsync(jsonNode);

            return OutputMessage;
        }
        finally
        {
            LeaveNode(this);
        }
    }

    private static JsonObject JsonErrorMessage(string parametersJsonString, string errorMessage)
    {

        var errorNode = new JsonObject
        {
            ["message"] = errorMessage
        };

        var jsonMessage = new JsonObject
        {
            ["Input"] = JsonNode.Parse(parametersJsonString),
            ["Error"] = errorNode
        };

        return jsonMessage;
    }


    public override async Task DisplayNodeConfigurationPopup()
    {
        var configurationPopupViewModel = AppService.GetService<Ks0212ConfigurePopupViewModel>();

        if (configurationPopupViewModel is null)
        {
            Debug.WriteLine(
                "Ks0212ConfigurePopupViewModel is null. That means NO configuration popup will be shown. This should be not happen. Remove DisplayNodeConfigurationPopup for the node");
            return;
        }

        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(NodeKs0212)] = this
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        await PopupService.ShowPopupAsync<Ks0212ConfigurePopupViewModel>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);
    }
}

public partial class RelaySettings : ObservableObject
{
    [ObservableProperty] private bool one;

    [ObservableProperty] private bool two;

    [ObservableProperty] private bool three;

    [ObservableProperty] private bool four;

    public RelaySettings()
    {
    }

    public RelaySettings(JsonElement element)
    {
        if (!element.TryGetProperty("RelaySettings", out var settings) ||
            settings.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("RelaySettings object not found or invalid");
        }

        One = (settings.TryGetProperty("one", out var oneProperty) && oneProperty.ValueKind == JsonValueKind.True ||
               oneProperty.ValueKind == JsonValueKind.False) && oneProperty.GetBoolean();

        Two = (settings.TryGetProperty("two", out var twoProperty) && twoProperty.ValueKind == JsonValueKind.True ||
               twoProperty.ValueKind == JsonValueKind.False) && twoProperty.GetBoolean();

        Three = (settings.TryGetProperty("three", out var threeProperty) &&
                 threeProperty.ValueKind == JsonValueKind.True ||
                 threeProperty.ValueKind == JsonValueKind.False) && threeProperty.GetBoolean();

        Four = (settings.TryGetProperty("four", out var fourProperty) && fourProperty.ValueKind == JsonValueKind.True ||
                fourProperty.ValueKind == JsonValueKind.False) && fourProperty.GetBoolean();
    }
}