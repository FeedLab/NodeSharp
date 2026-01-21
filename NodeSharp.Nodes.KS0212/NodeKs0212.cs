using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.KS0212.ViewModel;

namespace NodeSharp.Nodes.KS0212;

public class NodeKs0212 : BaseNode
{
    public RelaySettings InitialRelaySettings { get; }
    public RelaySettings CurrentRelaySettings { get; set; }

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
        InitialRelaySettings = new RelaySettings();
        CurrentRelaySettings = new RelaySettings();

        BoxDimension = new Rect(0, 0, 250, 235);

        Outputs.Clear();

        Outputs.Add(new Output(Guid.CreateVersion7(), "Status", [], new Point(50, 30)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 1", [], new Point(50, 70)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 2", [], new Point(50, 100)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 3", [], new Point(50, 130)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 4", [], new Point(50, 160)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Error", [], new Point(50, 200)));

        Inputs.Clear();
        Inputs.Add(new Input(Guid.CreateVersion7(), "Input", [], new Point(0, BoxDimension.Height / 2)));
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
            InitialRelaySettings = new RelaySettings(nodeElement, "InitialRelaySettings");
            CurrentRelaySettings = new RelaySettings();
        }
        catch (Exception e)
        {
            throw new NodeParseException(this, nameof(RelaySettings), e);
        }
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {
        var stopwatch = EnterNode(this);

        try
        {
            var jsonInput = await base.RunFromInput(parentNode, inputJsonString);


            var jsonElement = JsonDocument.Parse(inputJsonString).RootElement;

            var newRelaySettings = new RelaySettings(jsonElement, "Payload");

            if (!jsonElement.TryGetPropertyIgnoreCase("Payload", out var settings) ||
                settings.ValueKind != JsonValueKind.Object)
            {
                var errorMessage = $"An error occurred. Make sure 'msg.Payload' is present in the input JSON.";

                throw new NodeParseException(errorMessage, "Payload");
            }

            if (newRelaySettings.RelayOne is not null && newRelaySettings.RelayOne != CurrentRelaySettings.RelayOne)
            {
                var relayOne = CreateRelayMessage((bool)newRelaySettings.RelayOne);
                await SendToConnectedChildrenAsync(relayOne, Outputs[1]);
                CurrentRelaySettings.RelayOne = newRelaySettings.RelayOne;
            }

            if (newRelaySettings.RelayTwo is not null && newRelaySettings.RelayTwo != CurrentRelaySettings.RelayTwo)
            {
                var relayTwo = CreateRelayMessage((bool)newRelaySettings.RelayTwo!);
                await SendToConnectedChildrenAsync(relayTwo, Outputs[2]);
                CurrentRelaySettings.RelayTwo = newRelaySettings.RelayTwo;
            }

            if (newRelaySettings.RelayThree is not null &&
                newRelaySettings.RelayThree != CurrentRelaySettings.RelayThree)
            {
                var relayThree = CreateRelayMessage((bool)newRelaySettings.RelayThree!);
                await SendToConnectedChildrenAsync(relayThree, Outputs[3]);
                CurrentRelaySettings.RelayThree = newRelaySettings.RelayThree;
            }

            if (newRelaySettings.RelayFour is not null && newRelaySettings.RelayFour != CurrentRelaySettings.RelayFour)
            {
                var relayFour = CreateRelayMessage((bool)newRelaySettings.RelayFour!);
                await SendToConnectedChildrenAsync(relayFour, Outputs[4]);
                CurrentRelaySettings.RelayFour = newRelaySettings.RelayFour;
            }

            var relaySettingsJson = new JsonObject
            {
                ["RelayOne"] = CurrentRelaySettings.RelayOne,
                ["RelayTwo"] = CurrentRelaySettings.RelayTwo,
                ["RelayThree"] = CurrentRelaySettings.RelayThree,
                ["RelayFour"] = CurrentRelaySettings.RelayFour,
                ["Input"] = jsonInput
            };

            await SendToConnectedChildrenAsync(relaySettingsJson, Outputs[0]);

            return relaySettingsJson;
        }
        catch (Exception ex)
        {
            var jsonErrorMessage = JsonErrorMessage(inputJsonString, ex.Message);

            await SendToConnectedChildrenAsync(jsonErrorMessage, Outputs[5]);

            throw;
        }
        finally
        {
            LeaveNode(this, stopwatch);
        }
    }

    private JsonObject CreateRelayMessage(bool relayStatus)
    {
        var relayNode = new JsonObject
        {
            ["Relay"] = relayStatus
        };

        return relayNode;
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
    
    public override void RecalculateInputNodes(double height)
    {
        Inputs[0].StartPosition = new Point(1, height / 2);
    }
    
    public override void RecalculateOutputNodes(double height)
    {
        Outputs[0].StartPosition = new Point(1, height / 2);
    }
}

public partial class RelaySettings : ObservableObject
{
    [ObservableProperty] private bool? relayOne;

    [ObservableProperty] private bool? relayTwo;

    [ObservableProperty] private bool? relayThree;

    [ObservableProperty] private bool? relayFour;

    public RelaySettings()
    {
    }

    public RelaySettings(JsonElement element, string nodeName)
    {
        if (!element.TryGetPropertyIgnoreCase(nodeName, out var settings) ||
            settings.ValueKind != JsonValueKind.Object)
        {
            var errorMessage = $"An error occurred. Make sure 'msg.{nodeName}' is present in the input JSON.";

            throw new NodeParseException(errorMessage, nodeName);
        }

        RelayOne = settings.TryGetPropertyIgnoreCase(nameof(RelayOne), out var oneProperty) &&
                   oneProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? oneProperty.GetBoolean()
            : null;

        RelayTwo = settings.TryGetPropertyIgnoreCase(nameof(RelayTwo), out var twoProperty) &&
                   twoProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? twoProperty.GetBoolean()
            : null;

        RelayThree = settings.TryGetPropertyIgnoreCase(nameof(RelayThree), out var threeProperty) &&
                     threeProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? threeProperty.GetBoolean()
            : null;

        RelayFour = settings.TryGetPropertyIgnoreCase(nameof(RelayFour), out var fourProperty) &&
                    fourProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? fourProperty.GetBoolean()
            : null;
    }
}