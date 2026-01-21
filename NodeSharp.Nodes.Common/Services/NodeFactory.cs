using System.Text.Json;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.Common.Services;

public class NodeFactory
{
    private readonly Storage storage;

    public NodeFactory(Storage storage)
    {
        this.storage = storage;
    }

    public BaseNode CreateNode(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition,
        int yPosition,
        Storage storageParam)
    {
        var nodeType = GetNodeType(typeId);

        var parameters = new object[]
        {
            nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, storageParam
        };

        var instance = Activator.CreateInstance(nodeType, parameters);

        if (instance is not BaseNode node)
        {
            throw new InvalidOperationException($"Failed to create node of type: {typeId}");
        }

        return node;
    }

    public BaseNode CreateNodeFromJson(
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
    {
        var nodeType = GetNodeType(typeId);

        var parameters = new object[]
        {
            nodes, id, typeId, name, isEnabled, activateOnStart, xPosition, yPosition, outputs, inputs, nodeElement
        };

        var instance = Activator.CreateInstance(nodeType, parameters);

        if (instance is not BaseNode node)
        {
            throw new InvalidOperationException($"Failed to create node of type: {typeId}");
        }

        return node;
    }

    private Type GetNodeType(string typeId)
    {
        if (!storage.GetNodeInformation().TryGetNodeType(typeId, out var nodeType))
        {
            throw new InvalidOperationException($"Unknown node type: {typeId}");
        }

        return nodeType;
    }
}
