using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.Inject;

public class NodeInject : BaseNode
{
    public ActivateAfter ActivateAfter { get; }
    public Repeat Repeat { get; }
    public List<Parameter> Parameters { get; }

    public NodeInject(
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
        Repeat = new Repeat("Second", 10, false);
        ActivateAfter = new ActivateAfter("Second", 1);
        Parameters = new List<Parameter>();
    }

    public NodeInject(
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
            Repeat = new Repeat(
                nodeElement.GetProperty("Repeat").GetProperty("Type").GetString()!,
                nodeElement.GetProperty("Repeat").GetProperty("Value").GetInt32(),
                nodeElement.GetProperty("Repeat").GetProperty("IsEnabled").GetBoolean()
            );
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(this, nameof(Repeat), e);
        }

        try
        {
            ActivateAfter = new ActivateAfter(
                nodeElement.GetProperty("ActivateAfter").GetProperty("Type").GetString()!,
                nodeElement.GetProperty("ActivateAfter").GetProperty("Value").GetInt32());
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(this, nameof(ActivateAfter), e);
        }

        try
        {
            Parameters = nodeElement.GetProperty("Parameters").EnumerateArray()
                .Select(p =>
                {
                    var source = p.TryGetProperty("Source", out var sourceProp)
                        ? sourceProp.GetString() ?? "primitive"
                        : "primitive";

                    var declaredType = p.GetProperty("Type").GetString() ?? "string";
                    var effectiveType = string.Equals(source, "environment", StringComparison.OrdinalIgnoreCase)
                        ? "environment"
                        : declaredType;

                    return new Parameter(
                        p.GetProperty("Name").GetString() ?? "Unknown",
                        effectiveType,
                        source,
                        p.TryGetProperty("Value", out var val) ? (val.GetString() ?? "") : ""
                    );
                }).ToList();
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(this, nameof(Parameters), e);
        }
    }

    public override Task Run()
    {
            EnterNode(this);

            try
            {
                if (!ActivateOnStart)
                {
                    return Task.CompletedTask;
                }
                
                
                var _ =Task.Run(async () =>
                {
                    if (ActivateAfter.Value > 0)
                    {
                        Debug.WriteLine($"Inject: Delay is enabled. Waiting {ActivateAfter.ActivateAfterMilliseconds} milliseconds before execute.");
                        
                        var activateAfterMs =  ActivateAfter.Type.ConvertTimeToMilliseconds(ActivateAfter.Value);
                        await Task.Delay(activateAfterMs);
                    }
                    
                    if (Repeat.IsEnabled)
                    {
                        Debug.WriteLine("Inject: Starting repeating");

                        var repeatMs =  Repeat.Type.ConvertTimeToMilliseconds(Repeat.Value);
                        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(repeatMs));

                        try
                        {
                            do
                            {
                                await base.Run();
                                var parametersJsonString = BuildParametersJson(Parameters);
                                await SendToConnectedChildrenAsync(parametersJsonString);
                            } while (await timer.WaitForNextTickAsync(Cts.Token));
                        }
                        catch (OperationCanceledException)
                        {
                            Debug.WriteLine("Inject timer cancelled.");
                        }
                        finally
                        {
                            timer?.Dispose();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Inject: Starting once.");
                        await base.Run();
                        var parametersJsonString = BuildParametersJson(Parameters);
                        await SendToConnectedChildrenAsync(parametersJsonString);
                    }
                });
            }
            finally
            {
                LeaveNode(this);
            }

            return Task.CompletedTask;
    }


    private static string BuildParametersJson(IList<Parameter> parameters)
    {
        var sb = new StringBuilder();
        sb.Append("{\"Parameters\": [");

        var isFirstItem = true;
        foreach (var parameter in parameters)
        {
            if (!isFirstItem)
            {
                sb.Append(',');
            }

            if (parameter.Source.Equals("primitive", StringComparison.OrdinalIgnoreCase))
            {
                AppendParameterJson(sb, parameter);
            }
            else if (parameter.Source.Equals("environment", StringComparison.OrdinalIgnoreCase))
            {
                var envValue = GetRequiredEnvironmentVariable(parameter.Value);

                if (IsNumeric(envValue))
                {
                    var param = new Parameter(parameter.Name, "number", "primitive", envValue);
                    AppendParameterJson(sb, param);
                }
                else
                {
                    var param = new Parameter(parameter.Name, "string", "primitive", envValue);
                    AppendParameterJson(sb, param);
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid parameter source: {parameter.Source}");
            }

            isFirstItem = false;
        }

        sb.Append("]}");
        return sb.ToString();
    }

    private static bool IsNumeric(string? value)
    {
        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out _);
    }

    private static void AppendParameterJson(StringBuilder sb, Parameter parameter)
    {
        var name = parameter.Name;
        var type = parameter.Type.Trim().ToLowerInvariant();
        var source = parameter.Source.Trim().ToLowerInvariant();

        sb.Append('{');
        //       sb.Append($"\"name\": \"{name}\",");

        switch (type)
        {
            case "string":
                sb.Append($"\"{parameter.Name}\": \"{parameter.Value}\"");
                break;

            case "number":
                if (!decimal.TryParse(parameter.Value, NumberStyles.Number, CultureInfo.InvariantCulture,
                        out var number))
                {
                    throw new InvalidOperationException(
                        $"Parameter '{name}' has invalid number value '{parameter.Value}'. Expected InvariantCulture numeric format.");
                }

                sb.Append($"\"{parameter.Name}\": {number.ToString(CultureInfo.InvariantCulture)}");
                break;

            case "boolean":
                if (!bool.TryParse(parameter.Value, out var boolean))
                {
                    throw new InvalidOperationException(
                        $"Parameter '{name}' has invalid boolean value '{parameter.Value}'. Expected 'true' or 'false'.");
                }

                sb.Append($"\"{parameter.Name}\": {(boolean ? "true" : "false")}");
                break;

            case "environment":
                var envValue = GetRequiredEnvironmentVariable(parameter.Value);
                sb.Append($"\"{parameter.Name}\": \"{envValue}\"");
                break;

            default:
                throw new InvalidOperationException($"Invalid parameter type: {parameter.Type}");
        }

        sb.Append('}');
    }

    private static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{variableName}' is not set (or is empty).");
        }

        return value;
    }
}

public class ActivateAfter
{
    public string Type { get; }
    public int Value { get; }

    [JsonIgnore] public int ActivateAfterMilliseconds { get; }

    public ActivateAfter(string type, int value)
    {
        Type = type;
        Value = value;

        ActivateAfterMilliseconds = Type.ConvertTimeToMilliseconds(Value);
    }
}

public class Repeat
{
    public string Type { get; }
 
    public int Value { get; }

    public bool IsEnabled { get; }

    public int RepeatAfterMilliseconds { get; }

    public Repeat(string type, int value, bool isEnabled)
    {
        Type = type;
        Value = value;
        IsEnabled = isEnabled;
        
        RepeatAfterMilliseconds = Type.ConvertTimeToMilliseconds(Value);
    }

}

public class Parameter
{
    public string Name { get; }
    public string Type { get; }
    public string Source { get; }
    public string Value { get; }

    public Parameter(string name, string type, string source, string value)
    {
        Name = name;
        Type = type;
        Source = source;
        Value = value;
    }
}