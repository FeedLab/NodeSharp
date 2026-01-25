using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Function;

public class NodeFunction : BaseNode
{
    [JsonInclude] public FunctionData FunctionData { get; set; }

    public NodeFunction(
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
        FunctionData = new FunctionData();

        var statusBodyHeight =  InitializeDimensions();
        
        Inputs.Clear();
        Outputs.Clear();

        Inputs.Add(new Input(Guid.CreateVersion7(), "Input", [], new Point(0, (BoxDimension.Height - statusBodyHeight) / 2)));
        Outputs.Add(new Output(Guid.CreateVersion7(), "Output", [], new Point(0, (BoxDimension.Height - statusBodyHeight) / 2)));
        
    }

    private double InitializeDimensions()
    {
        const double height = 70;
        const double width = 280;
        const double statusBodyHeight = 12;

        BoxDimension = new Rect(0, 0, width, height);

        return statusBodyHeight;
    }
    
    public NodeFunction(
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
        
        if (!nodeElement.TryGetProperty("FunctionData", out var functionProp) ||
            functionProp.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("FunctionData object not found or invalid");
        }

        FunctionData = new FunctionData(functionProp);
    }

    protected override async Task<JsonNode?> RunFromInput(BaseNode parentNode, string inputJsonString)
    {    
        var stopwatch = EnterNode(this);
        
        try
        {
            var fromInput = await base.RunFromInput(parentNode, inputJsonString);

            var runStatus = FunctionData.ExecuteScript(inputJsonString);

            var updatedJsonString = runStatus.Output ?? inputJsonString;

            MainThread.BeginInvokeOnMainThread(() => { BoxNodeStatus.Message = DateTime.Now.ToString("HH:mm:ss"); });

            await SendToConnectedChildrenAsync(updatedJsonString);

            return await Task.FromResult(fromInput);
        }
        finally
        {
            LeaveNode(this, stopwatch);
        }
    }

    public override async Task DisplayNodeConfigurationPopup()
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(NodeFunction)] = this
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        await PopupService.ShowPopupAsync<FunctionConfigurePopupViewModel>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);
    }
}

public partial class FunctionData : ObservableObject
{
    private MethodInfo? method;

    [ObservableProperty] private string sourceCode;

    partial void OnSourceCodeChanged(string value)
    {
        Task.Run(CompileScript);
    }

    public (bool Success, string? Output, System.Exception? Error) ExecuteScript(string json)
    {
        if (method == null)
            return (false, null, new InvalidOperationException("Runner.Execute method not found"));

        try
        {
            var output = method.Invoke(null, [json]);
            return (true, output?.ToString(), null);
        }
        catch (TargetInvocationException tie)
        {
            // unwrap inner exception thrown by the script
            return (false, null, tie.InnerException ?? tie);
        }
        catch (System.Exception ex)
        {
            return (false, null, ex);
        }
    }

    public const string MessageTemplate =
        "using System;\n\rusing System.Dynamic;\n\rusing Newtonsoft.Json;\n\n\rpublic class Runner \n\r{\n\r    public static string Execute(string json) \n\r    {\n\r        dynamic msg = JsonConvert.DeserializeObject<ExpandoObject>(json);\n\n        // Serialize back to JSON\n\r\n\r        ##@@##\n\n        string updatedJson = JsonConvert.SerializeObject(msg, Formatting.Indented);\n\r\n\r        return updatedJson;\n\r    }\n\r}";


    public void CompileScript()
    {
        var roslynHelper = new RoslynHelper(MessageTemplate, SourceCode);
        roslynHelper.CompileScript();
        method = roslynHelper.GetExecutionMethod();
    }

    public FunctionData(JsonElement element)
    {
        var existsSourceCode = element.TryGetProperty("SourceCode", out var sourceProp);
        var sourceCode = !existsSourceCode || sourceProp.ValueKind != JsonValueKind.String
            ? throw new NodeParseException(nameof(SourceCode), "Must exists and be of type string")
            : sourceProp.GetString();

        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            throw new NodeParseException(nameof(SourceCode), "SourceCode can not be empty or null");
        }

        SourceCode = sourceCode!;

        Task.Run(CompileScript);
    }

    /*START_USER_CODE*/
    public FunctionData(
        string sourceCode =
            "msg.extraInfo = \"added at runtime\";\n\rmsg.number = new ExpandoObject();\n\rmsg.number.data = 123;")
    {
        SourceCode = sourceCode;

        Task.Run(CompileScript);
    }
}