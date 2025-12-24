using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NodeSharp.NodeEngine;

public class NodeInject : BaseNode
{
    public ActivateAfter ActivateAfter { get; }
    public Parameter[] Parameters { get; }

    public NodeInject(
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
        ActivateAfter = new ActivateAfter(
            nodeElement.GetProperty("ActivateAfter").GetProperty("Type").GetString()!,
            nodeElement.GetProperty("ActivateAfter").GetProperty("Value").GetInt32());

        Parameters = nodeElement.GetProperty("Parameters").EnumerateArray()
            .Select(p =>
            {
                var source = p.TryGetProperty("source", out var sourceProp)
                    ? sourceProp.GetString()
                    : "primitive";

                var declaredType = p.GetProperty("Type").GetString()!;
                var effectiveType = string.Equals(source, "environment", StringComparison.OrdinalIgnoreCase)
                    ? "environment"
                    : declaredType;

                return new Parameter(
                    p.GetProperty("Name").GetString()!,
                    effectiveType,
                    p.GetProperty("Source").GetString()!,
                    p.TryGetProperty("Value", out var val) ? val.GetString()! : ""
                );
            }).ToArray();
    }



    public override async Task Run()
    {
        if (!ActivateOnStart)
        {
            return;
        }

        await base.Run();

        var parametersJsonString = BuildParametersJson(Parameters);

        await SendToConnectedChildrenAsync(parametersJsonString);
    }

    private static string BuildParametersJson(Parameter[] parameters)
    {
        var sb = new StringBuilder();
        sb.Append('[');

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
                var param = new Parameter(parameter.Name, parameter.Type, "primitive", envValue);

                AppendParameterJson(sb, param);
            }
            else
            {
                throw new InvalidOperationException($"Invalid parameter source: {parameter.Source}");
            }

            isFirstItem = false;
        }

        sb.Append(']');
        return sb.ToString();
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