using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Exception;
using NodeSharp.NodeEngine.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace NodeSharp.NodeEngine.Node;

public class NodeFunction : BaseNode
{
    [JsonInclude] private FunctionData FunctionData { get; set; }

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

            //         var codeUpdated = string.Format(FunctionData.SourceCodeTemplate, parametersJsonString);

            var updatedJsonString = await ExecuteScriptAsync(FunctionData.SourceCodeTemplate);

            await SendToConnectedChildrenAsync(updatedJsonString);

            return await Task.FromResult(updatedJsonString);
        }
        finally
        {
            LeaveNode(this);
        }
    }

    private async Task<string?> ExecuteScriptAsync(string code)
    {
        // var result = await CSharpScript
        //     .RunAsync(code, ScriptOptions.Default
        //         .AddReferences(
        //             typeof(object).Assembly, 
        //             typeof(JsonConvert).Assembly,
        //             typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly)
        //         .AddImports("System", "System.Dynamic", "Newtonsoft.Json"));

        var result = await FunctionData.Script.RunAsync(cancellationToken: Cts.Token);
        
        return result.ReturnValue?.ToString();
    }
}

public class FunctionData
{
    public Script<object> Script;
    public string SourceCode { get; set; }
    public string SourceCodeTemplate { get; set; }

    private void CompileScript()
    {
        Script = CSharpScript.Create(MessageTemplate,
            ScriptOptions.Default
                .AddReferences(
                    typeof(object).Assembly, 
                    typeof(JsonConvert).Assembly,
                    typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly)
                .AddImports("System", "System.Dynamic", "Newtonsoft.Json"));
        
        Script.Compile();
    }

    private const string MessageTemplate = $$"""
                                             string json = @"{
                                               ""payload"": {
                                                 ""deviceId"": ""SR4314-F03"",
                                                 ""temperature"": 23.3,
                                                 ""ema"": 23.3
                                               },
                                               ""_msgid"": ""7e72d0c68a4535f4""
                                             }";

                                             // Deserialize into dynamic ExpandoObject
                                             dynamic msg = JsonConvert.DeserializeObject<ExpandoObject>(json);

                                             // Add new fields dynamically
                                             msg.payload.newField = "hello world";
                                             msg.payload.calibrationOffset = 1.25;
                                             msg.extraInfo = "added at runtime";

                                             // Access them
                                             Console.WriteLine(msg.payload.newField);          // hello world
                                             Console.WriteLine(msg.payload.calibrationOffset); // 1.25
                                             Console.WriteLine(msg.extraInfo);                 // added at runtime

                                             // Serialize back to JSON
                                             string updatedJson = JsonConvert.SerializeObject(msg, Formatting.Indented);
                                             Console.WriteLine(updatedJson);

                                             return updatedJson;
                                             """;


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

        var existsSourceCodeTemplate = element.TryGetProperty("SourceCodeTemplate", out var sourceTemplateProp);
        var sourceCodeTemplate = !existsSourceCodeTemplate || sourceTemplateProp.ValueKind != JsonValueKind.String
            ? throw new NodeParseException(nameof(SourceCodeTemplate), "Must exists and be of type string")
            : sourceTemplateProp.GetString();

        if (string.IsNullOrWhiteSpace(sourceCodeTemplate))
        {
            throw new NodeParseException(nameof(SourceCode), "SourceCode can not be empty or null");
        }

        // SourceCodeTemplate = sourceCodeTemplate;
        SourceCodeTemplate = MessageTemplate;

        CompileScript();
    }

    /*START_USER_CODE*/
    public FunctionData(string sourceCode = "int index = 100; return index;")
    {
        SourceCode = sourceCode;
        SourceCodeTemplate = MessageTemplate;
    }
}