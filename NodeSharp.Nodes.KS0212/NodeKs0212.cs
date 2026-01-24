using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
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

        const double height = 225;
        const double width = 250;
        const double statusBodyHeight = 12;
        const double anchorWidth = 60;
        const double paddingWidth = 4;

        BoxDimension = new Rect(0, 0, width + paddingWidth + paddingWidth, height);
      //  BoxBodyDimension = new Rect(0, 0, width - anchorWidth - anchorWidth, height - statusBodyHeight);

        Outputs.Clear();

        Outputs.Add(new Output(Guid.CreateVersion7(), "Status", [], new Point(0, 30)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 1", [], new Point(0, 70)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 2", [], new Point(0, 100)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 3", [], new Point(0, 130)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Relay 4", [], new Point(0, 160)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Error", [], new Point(0, 200)));

        Inputs.Clear();
        Inputs.Add(new Input(Guid.CreateVersion7(), "Input", [], new Point(0, (height - statusBodyHeight) / 2)));
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
            BoxDimension = new Rect(0, 0, 250, 235);
        }
        catch (Exception e)
        {
            throw new NodeParseException(this, nameof(RelaySettings), e);
        }
    }

    public override void Reset()
    {
        base.Reset();
        
        if (BoxNodeStatusComponent is MultiBoxComponent statusComponent)
        {
            statusComponent.ViewModel.TurnAllOff();
        }
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {
        var stopwatch = EnterNode(this);

        try
        {
            var jsonInput = await base.RunFromInput(parentNode, inputJsonString);


            var jsonElement = JsonDocument.Parse(inputJsonString).RootElement;

            // var newRelaySettings = new RelaySettings(jsonElement, "Payload");

            if (!jsonElement.TryGetPropertyIgnoreCase("Payload", out var settings) ||
                settings.ValueKind != JsonValueKind.Object)
            {
                var errorMessage = $"An error occurred. Make sure 'msg.Payload' is present in the input JSON.";

                throw new NodeParseException(errorMessage, "Payload");
            }

            await ProcessRelayAsync(settings, nameof(RelaySettings.RelayOne),
                () => CurrentRelaySettings.RelayOne, v => CurrentRelaySettings.RelayOne = v, 1, 0);
            await ProcessRelayAsync(settings, nameof(RelaySettings.RelayTwo),
                () => CurrentRelaySettings.RelayTwo, v => CurrentRelaySettings.RelayTwo = v, 2, 1);
            await ProcessRelayAsync(settings, nameof(RelaySettings.RelayThree),
                () => CurrentRelaySettings.RelayThree, v => CurrentRelaySettings.RelayThree = v, 3, 2);
            await ProcessRelayAsync(settings, nameof(RelaySettings.RelayFour),
                () => CurrentRelaySettings.RelayFour, v => CurrentRelaySettings.RelayFour = v, 4, 3);


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

    private async Task ProcessRelayAsync(JsonElement settings, string propertyName,
        Func<bool> getCurrentValue, Action<bool> setCurrentValue, int outputIndex, int relayIndex)
    {
        var relayValue = settings.TryGetPropertyIgnoreCase(propertyName, out var property) &&
                         property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : (bool?)null;

        if (relayValue.HasValue && relayValue.Value != getCurrentValue())
        {
            var relayMessage = CreateRelayMessage(relayValue.Value);
            await SendToConnectedChildrenAsync(relayMessage, Outputs[outputIndex]);
            setCurrentValue(relayValue.Value);

            if (BoxNodeStatusComponent is MultiBoxComponent statusComponent)
            {
                if (relayValue.Value)
                {
                    statusComponent.ViewModel.TurnOn(relayIndex);
                }
                else
                {
                    statusComponent.ViewModel.TurnOff(relayIndex);
                }
            }
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

    // public override void RecalculateInputNodes(double height)
    // {
    //     Inputs[0].StartPosition = new Point(1, height / 2);
    // }
    //
    // public override void RecalculateOutputNodes(double height)
    // {
    //     Outputs[0].StartPosition = new Point(1, height / 2);
    // }
}

public partial class RelaySettings : ObservableObject
{
    [ObservableProperty] private bool relayOne;

    [ObservableProperty] private bool relayTwo;

    [ObservableProperty] private bool relayThree;

    [ObservableProperty] private bool relayFour;

    public RelaySettings()
    {
        relayOne = false;
        relayTwo = false;
        relayThree = false;
        relayFour = false;
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
            : throw new InvalidOperationException($"Invalid value for 'RelayOne' in 'msg.{nodeName}'");

        RelayTwo = settings.TryGetPropertyIgnoreCase(nameof(RelayTwo), out var twoProperty) &&
                   twoProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? twoProperty.GetBoolean()
            : throw new InvalidOperationException($"Invalid value for 'RelayTwo' in 'msg.{nodeName}'");

        RelayThree = settings.TryGetPropertyIgnoreCase(nameof(RelayThree), out var threeProperty) &&
                     threeProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? threeProperty.GetBoolean()
            : throw new InvalidOperationException($"Invalid value for 'RelayThree' in 'msg.{nodeName}'");

        RelayFour = settings.TryGetPropertyIgnoreCase(nameof(RelayFour), out var fourProperty) &&
                    fourProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? fourProperty.GetBoolean()
            : throw new InvalidOperationException($"Invalid value for 'RelayFour' in 'msg.{nodeName}'");
    }
}