using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Exception;
using NodeSharp.NodeEngine.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using NodeSharp.NodeEngine.Helper;

namespace NodeSharp.NodeEngine.Node;

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
        FunctionData functionData)
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
        FunctionData = functionData;
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
        try
        {
            if (!nodeElement.TryGetProperty("FunctionData", out var functionProp) ||
                functionProp.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("FunctionData object not found or invalid");
            }

            FunctionData = new FunctionData(functionProp);
        }
        catch (System.Exception e)
        {
            //   throw new NodeParseException(this, nameof(functionData), e);
        }
    }

    public override async Task<string> RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        try
        {
            EnterNode(this);

            await base.RunFromInput(parentNode, parametersJsonString);

            var runStatus = FunctionData.ExecuteScript(parametersJsonString);

            var updatedJsonString = runStatus.Output ?? parametersJsonString;

            await SendToConnectedChildrenAsync(updatedJsonString);

            return await Task.FromResult(updatedJsonString);
        }
        finally
        {
            LeaveNode(this);
        }
    }
}

public class FunctionData
{
    private MethodInfo? method;

    public string SourceCode { get; set; }

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
    public FunctionData(string sourceCode = "msg.extraInfo = \"added at runtime\";\n\rmsg.number = new ExpandoObject();\n\rmsg.number.data = 123;")
    {
        SourceCode = sourceCode;
        
        Task.Run(CompileScript);
    }
}