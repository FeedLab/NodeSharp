using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Model;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine;

public class NodeIo(Storage storage)
{
    private readonly BaseNodeList nodes = [];
    private string? fileNameSaved;

    public BaseNodeList Nodes => nodes;

    public string? FileNameSaved
    {
        get => fileNameSaved;
        set => fileNameSaved = value;
    }

    public async Task SaveToFileAsync()
    {
        if (fileNameSaved is null)
        {
            throw new InvalidOperationException("No file name exists. Call LoadFromFileAsync first.");
        }

        if (File.Exists(fileNameSaved))
        {
            File.Delete(fileNameSaved);
        }

        using var stream = File.Create(fileNameSaved);

        await SaveToFileAsync(stream);
    }

    public async Task SaveToFileAsync(Stream stream)
    {
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

    public async Task LoadFromFileAsync(StreamReader reader, string filePath)
    {
        fileNameSaved = filePath;

            var nodeDataJson = await reader.ReadToEndAsync();

            if (nodeDataJson is null)
            {
                throw new InvalidOperationException($"Node data JSON is null. File name is: {filePath}");
            }

            Clear();

            var document = JsonDocument.Parse(nodeDataJson);
            var nodesArray = document.RootElement.GetProperty("Nodes");

            ParseNodesFromJson(nodesArray);

            nodes.ValidateInputAndOutputNodes();

            Console.WriteLine($"Loaded {nodes.Count} nodes:");

            foreach (var node in nodes)
            {
                Console.WriteLine(
                    $"  - {node.Name} ({node.TypeId}): {node.Outputs.Count} outputs, {node.Inputs.Count} inputs");
            }
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
            var xPosition = nodeElement.TryGetProperty("X", out var xProp) &&
                            xProp.ValueKind == JsonValueKind.Number
                ? xProp.GetInt32()
                : 100;

            var yPosition = nodeElement.TryGetProperty("Y", out var yProp) &&
                            yProp.ValueKind == JsonValueKind.Number
                ? yProp.GetInt32()
                : 100;

            var isEnabled = ReadBool(nodeElement, preferredPropertyName: "IsEnabled",
                fallbackPropertyName: "Enabled");
            var activateOnStart = nodeElement.TryGetProperty("ActivateOnStart", out var activateOnStartProp) &&
                                  activateOnStartProp.ValueKind == JsonValueKind.True;

            var outputs = ParseOutputs(nodeElement.GetProperty("Outputs"));
            var inputs = ParseInputs(nodeElement.GetProperty("Inputs"));

            BaseNode node = typeId switch
            {
                "Inject" => new NodeInject(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition,
                    yPosition, outputs, inputs,
                    nodeElement),
                "Debug" => new NodeDebug(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition,
                    outputs, inputs,
                    nodeElement),
                "RandomNumber" => new NodeRandomNumber(nodes, id, typeId, name, isEnabled, activateOnStart,
                    xPosition, yPosition, outputs,
                    inputs, nodeElement),
                "Delay" => new NodeDelay(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition,
                    outputs, inputs,
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
        fileNameSaved = null;
    }

    public void Add(string nodeTypeName, double dropX, double dropY)
    {
        var id = $"{Guid.CreateVersion7()}";
        var typeId = nodeTypeName;
        var name = $"{nodeTypeName} {nodes.Count + 1}";
        var xPosition = (int)dropX;
        var yPosition = (int)dropY;
        var isEnabled = true;
        var activateOnStart = false;

        BaseNode node = nodeTypeName switch
        {
            "Inject" => new NodeInject(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage),
            "Debug" => new NodeDebug(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage),
            "RandomNumber" => new NodeRandomNumber(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage, new RandomDataPayload()),
            "Delay" => new NodeDelay(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage, new DelayPayload()),
            _ => throw new InvalidOperationException($"Unknown node type: {nodeTypeName}")
        };

        nodes.Add(node);
    }
}

public class Input(string name, IList<string> connectsToParentNodeId)
{
    public string Name { get; } = name;
    public IList<string> ConnectsToParentNodeId { get; } = connectsToParentNodeId;
}

public class Output(string name, IList<string> connectsToNodeId)
{
    public string Name { get; } = name;
    public IList<string> ConnectsToNodeId { get; } = connectsToNodeId;
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