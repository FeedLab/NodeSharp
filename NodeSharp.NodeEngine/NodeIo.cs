using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NodeSharp.NodeEngine.Node;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Inject;

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

    public bool IsFlowRunning { get; set; }

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

        try
        {
            ParseNodesFromJson(nodesArray);
        }
        catch (System.Exception e)
        {
            Debug.WriteLine(e.Message);

            throw;
        }

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
        await Task.Run(() =>
        {
            IsFlowRunning = true;

            _ = nodes.Run();
        });
    }


    public T? FindNodeFromId<T>(string id) where T : BaseNode => nodes.OfType<T>().SingleOrDefault(x => x.Id == id);

    void ParseNodesFromJson(JsonElement jsonElement)
    {
        foreach (var nodeElement in jsonElement.EnumerateArray())
        {
            try
            {
                var id = GetProperty(nodeElement, "Id").GetString()!;
                var typeId = GetProperty(nodeElement, "TypeId").GetString()!;
                var name = GetProperty(nodeElement, "Name").GetString()!;
                var xPosition = TryGetProperty(nodeElement, "X", out var xProp) &&
                                xProp.ValueKind == JsonValueKind.Number
                    ? xProp.GetInt32()
                    : 100;

                var yPosition = TryGetProperty(nodeElement, "Y", out var yProp) &&
                                yProp.ValueKind == JsonValueKind.Number
                    ? yProp.GetInt32()
                    : 100;

                var isEnabled = ReadBool(nodeElement, preferredPropertyName: "IsEnabled",
                    fallbackPropertyName: "Enabled");
                var activateOnStart = TryGetProperty(nodeElement, "ActivateOnStart", out var activateOnStartProp) &&
                                      activateOnStartProp.ValueKind == JsonValueKind.True;

                var outputs = ParseOutputs(GetProperty(nodeElement, "Outputs"));
                var inputs = ParseInputs(GetProperty(nodeElement, "Inputs"));


                BaseNode node = typeId switch
                {
                    "Inject" => new NodeInject(nodes, id, typeId, name, isEnabled, true, xPosition,
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
                    "Function" => new NodeFunction(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition,
                        outputs, inputs,
                        nodeElement),
                    _ => throw new InvalidOperationException($"Unknown TypeId: {typeId}")
                };

                nodes.Add(node);
            }
            catch (System.Exception e)
            {
                throw;
            }
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

    private static JsonElement GetProperty(JsonElement nodeElement, string propertyName)
    {
        try
        {
            return nodeElement.GetProperty(propertyName);
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(propertyName, e);
        }
    }

    private static bool TryGetProperty(JsonElement nodeElement, string propertyName, out JsonElement propertyValue)
    {
        try
        {
            return nodeElement.TryGetProperty(propertyName, out propertyValue);
        }
        catch (System.Exception e)
        {
            throw new NodeParseException(propertyName, e);
        }
    }

    static List<Output> ParseOutputs(JsonElement outputsElement)
    {
        // Supports:
        // 1) [{ "Name": "...", "ConnectsToNodeId": ["..."] }, ...]
        // 2) ["nodeId-1", "nodeId-2", ...]
        return outputsElement.ValueKind switch
        {
            JsonValueKind.Array when outputsElement.GetArrayLength() == 0 => [],

            JsonValueKind.Array when outputsElement[0].ValueKind == JsonValueKind.Object =>
                outputsElement.EnumerateArray()
                    .Select(o => new Output(
                        o.GetProperty("Name").GetString()!,
                        o.GetProperty("ConnectsToNodeId").EnumerateArray().Select(x => x.GetString()!).ToList()
                    ))
                    .ToList(),

            JsonValueKind.Array when outputsElement[0].ValueKind == JsonValueKind.String =>
                outputsElement.EnumerateArray()
                    .Select((nodeIdElement, index) => new Output(
                        $"Output {index + 1}",
                        new List<string> { nodeIdElement.GetString()! }
                    ))
                    .ToList(),

            _ => throw new InvalidOperationException(
                "Invalid 'Outputs' JSON shape. Expected array of objects or array of strings.")
        };
    }

    static List<Input> ParseInputs(JsonElement inputsElement)
    {
        return inputsElement.EnumerateArray()
            .Select(i => new Input(
                i.GetProperty("Name").GetString()!,
                i.GetProperty("ConnectsToParentNodeId").EnumerateArray().Select(x => x.GetString()!).ToArray()
            ))
            .ToList();
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
            "Inject" => new NodeInject(nodes, id, typeId, name, isEnabled, true, xPosition, yPosition,
                storage),
            "Debug" => new NodeDebug(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition,
                storage),
            "RandomNumber" => new NodeRandomNumber(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition,
                yPosition, storage, new RandomDataPayload()),
            "Delay" => new NodeDelay(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storage,
                new DelayPayload()),
            "Function" => new NodeFunction(nodes, id, typeId, name, isEnabled, activateOnStart, xPosition,
                yPosition, storage, new FunctionData()),
            _ => throw new InvalidOperationException($"Unknown node type: {nodeTypeName}")
        };

        nodes.Add(node);
    }

    public void Abort()
    {
        foreach (var node in Nodes)
        {
            node.Abort();
        }

        IsFlowRunning = false;
    }
}


