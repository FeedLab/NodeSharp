using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine;

public class Main
{
    private readonly BaseNodeList nodes = [];
    private string fileNameSaved;

    public BaseNodeList Nodes => nodes;

    public async Task SaveToFileAsync()
    {
        if (fileNameSaved is null)
        {
            throw new InvalidOperationException("No file name exists. Call LoadFromFileAsync first.");
        }

        await SaveToFileAsync(fileNameSaved);
    }

    public async Task SaveToFileAsync(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        
        using var stream = File.Create(fileName);
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles 
        };
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = options.WriteIndented });

        writer.WriteStartObject();
        writer.WritePropertyName("Nodes");

        writer.WriteStartArray();
        foreach (var node in nodes)
        {
            JsonSerializer.Serialize(writer, node, node.GetType(), options);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();

        await writer.FlushAsync();
    }
    public async Task LoadFromFileAsync(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"File not found: {fileName}");
        }

        fileNameSaved = fileName;

        var nodeDataJson = await File.ReadAllTextAsync(fileName);

        if (nodeDataJson is null)
        {
            throw new InvalidOperationException($"Node data JSON is null. File name is: {fileName}");
        }

        var document = JsonDocument.Parse(nodeDataJson);
        var nodesArray = document.RootElement.GetProperty("Nodes");

        ParseNodesFromJson(nodesArray);

        nodes.ValidateInputAndOutputNodes();

        Console.WriteLine($"Loaded {nodes.Count} nodes:");

        foreach (var node in nodes)
        {
            Console.WriteLine(
                $"  - {node.Name} ({node.TypeId}): {node.Outputs.Length} outputs, {node.Inputs.Length} inputs");
        }

        // return Task.CompletedTask;
    }

    public async Task Run()
    {
        await nodes.Run();
    }

    public T? FindNodeFromId<T>(string id) where T : BaseNode => nodes.OfType<T>().SingleOrDefault(x => x.Id == id);

    void ParseNodesFromJson(JsonElement jsonElement)
    {
        foreach (var nodeElement in jsonElement.EnumerateArray())
        {
            var id = nodeElement.GetProperty("Id").GetString()!;
            var typeId = nodeElement.GetProperty("TypeId").GetString()!;
            var name = nodeElement.GetProperty("Name").GetString()!;

            var isEnabled = ReadBool(nodeElement, preferredPropertyName: "IsEnabled", fallbackPropertyName: "Enabled");
            var activateOnStart = nodeElement.TryGetProperty("ActivateOnStart", out var activateOnStartProp) &&
                                  activateOnStartProp.ValueKind == JsonValueKind.True;

            var outputs = ParseOutputs(nodeElement.GetProperty("Outputs"));
            var inputs = ParseInputs(nodeElement.GetProperty("Inputs"));

            BaseNode node = typeId switch
            {
                "Inject" => new NodeInject(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs,
                    nodeElement),
                "Debug" => new NodeDebug(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs,
                    nodeElement),
                "RandomNumber" => new NodeRandomNumber(nodes, id, typeId, name, isEnabled, activateOnStart, outputs,
                    inputs, nodeElement),
                "Delay" => new NodeDelay(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs,
                    nodeElement),
                _ => throw new InvalidOperationException($"Unknown TypeId: {typeId}")
            };

            nodes.Add(node);
        }

        static bool ReadBool(JsonElement element, string preferredPropertyName, string fallbackPropertyName)
        {
            if (element.TryGetProperty(preferredPropertyName, out var preferred))
            {
                return preferred.GetBoolean();
            }

            return element.GetProperty(fallbackPropertyName).GetBoolean();
        }
    }

    static Output[] ParseOutputs(JsonElement outputsElement)
    {
        // Supports:
        // 1) [{ "Name": "...", "ConnectsToNodeId": ["..."] }, ...]
        // 2) ["nodeId-1", "nodeId-2", ...]
        return outputsElement.ValueKind switch
        {
            JsonValueKind.Array when outputsElement.GetArrayLength() == 0 => Array.Empty<Output>(),

            JsonValueKind.Array when outputsElement[0].ValueKind == JsonValueKind.Object =>
                outputsElement.EnumerateArray()
                    .Select(o => new Output(
                        o.GetProperty("Name").GetString()!,
                        o.GetProperty("ConnectsToNodeId").EnumerateArray().Select(x => x.GetString()!).ToArray()
                    ))
                    .ToArray(),

            JsonValueKind.Array when outputsElement[0].ValueKind == JsonValueKind.String =>
                outputsElement.EnumerateArray()
                    .Select((nodeIdElement, index) => new Output(
                        $"Output {index + 1}",
                        [nodeIdElement.GetString()!]
                    ))
                    .ToArray(),

            _ => throw new InvalidOperationException(
                "Invalid 'Outputs' JSON shape. Expected array of objects or array of strings.")
        };
    }

    static Input[] ParseInputs(JsonElement inputsElement)
    {
        return inputsElement.EnumerateArray()
            .Select(i => new Input(
                i.GetProperty("Name").GetString()!,
                i.GetProperty("ConnectsToParentNodeId").EnumerateArray().Select(x => x.GetString()!).ToArray()
            ))
            .ToArray();
    }

    public void Clear()
    {
        Nodes.Clear();
    }
}

public class Input
{
    public string Name { get; }
    public string[] ConnectsToParentNodeId { get; }

    public Input(string name, string[] connectsToParentNodeId)
    {
        Name = name;
        ConnectsToParentNodeId = connectsToParentNodeId;
    }
}

public class Output
{
    public string Name { get; }
    public string[] ConnectsToNodeId { get; }

    public Output(string name, string[] connectsToNodeId)
    {
        Name = name;
        ConnectsToNodeId = connectsToNodeId;
    }
}

public class ActivateAfter
{
    public string Type { get; }
    public int Value { get; }

    public ActivateAfter(string type, int value)
    {
        Type = type;
        Value = value;
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