using System.Diagnostics;
using System.Text;

namespace NodeSharp.NodeEngine.Node;

public abstract class BaseNode
{
    public BaseNodeList Nodes { get; }
    public string Id { get; }
    public string TypeId { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public bool ActivateOnStart { get; }
    public Output[] Outputs { get; }
    public Input[] Inputs { get; }

    protected BaseNode(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        Output[] outputs,
        Input[] inputs)
    {
        Nodes = nodes;
        Id = id;
        TypeId = typeId;
        Name = name;
        IsEnabled = isEnabled;
        ActivateOnStart = activateOnStart;
        Outputs = outputs;
        Inputs = inputs;
    }

    public virtual Task Run()
    {
        if (!ActivateOnStart)
        {
            return Task.CompletedTask;
        }

        Debug.WriteLine($"Node {FormatNode()} is activated on start");
        return Task.CompletedTask;
    }

    public virtual Task<string> RunFromInput(BaseNode parent, string parametersJsonString)
    {
        Debug.WriteLine($"Node {FormatNode()} has been activated by parent node {parent.FormatNode()}");
        return Task.FromResult(parametersJsonString);
    }

    // ... existing code ...

    protected Task SendToConnectedChildrenAsync(string parametersJsonString)
    {
        var tasks = new List<Task>();

        foreach (var output in Outputs)
        {
            foreach (var nodeId in output.ConnectsToNodeId)
            {
                var targetNode = Nodes.Find(f => f.Id == nodeId);
                if (targetNode is null)
                {
                    throw new InvalidOperationException($"Node not found: {nodeId}");
                }

                tasks.Add(targetNode.RunFromInput(this, parametersJsonString));
            }
        }

        return Task.WhenAll(tasks);
    }

    public void ValidateInputAndOutput()
    {
        Debug.WriteLine($"Validating NodeInject: {Name} (Id: {Id})");
        ValidateNodeId();

        ValidateInputConnections(Nodes);
        ValidateOutputConnections(Nodes);

        Debug.WriteLine($"NodeInject {Name} validation completed successfully");
    }

    private void ValidateNodeId()
    {
        Debug.WriteLine($"Validating NodeId: {Id}");
        if (!Guid.TryParse(Id, out _))
        {
            Debug.WriteLine($"NodeId '{Id}' is not a valid GUID - throwing exception");
            throw new InvalidOperationException($"NodeId '{Id}' is not a valid GUID.");
        }

        Debug.WriteLine("NodeId is valid");
    }

    private void ValidateOutputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Outputs.Length} output connections");

        ValidateConnections(
            baseNodeList,
            connections: Outputs.SelectMany(o => o.ConnectsToNodeId.Select(nodeId => (PortName: o.Name, NodeId: nodeId))),
            idLabel: "Output ConnectsToNodeId",
            missingNodeMessage: (portName, nodeId) => $"Output '{portName}' connects to non-existing node '{nodeId}'. ");
    }

    private void ValidateInputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Inputs.Length} input connections");

        ValidateConnections(
            baseNodeList,
            connections: Inputs.SelectMany(i => i.ConnectsToParentNodeId.Select(nodeId => (PortName: i.Name, NodeId: nodeId))),
            idLabel: "Input ConnectsToParentNodeId",
            missingNodeMessage: (portName, nodeId) => $"Input '{portName}' connects to non-existing parent node '{nodeId}'. ");
    }

    private void ValidateConnections(
        BaseNodeList baseNodeList,
        IEnumerable<(string PortName, string NodeId)> connections,
        string idLabel,
        Func<string, string, string> missingNodeMessage)
    {
        var errors = new StringBuilder();

        foreach (var (portName, nodeId) in connections)
        {
            Debug.WriteLine($"Checking connection for '{portName}': {nodeId}");

            if (!Guid.TryParse(nodeId, out _))
            {
                Debug.WriteLine($"Invalid GUID: {nodeId}");
                errors.Append($"{idLabel} '{nodeId}' is not a valid GUID. ");
                continue;
            }

            if (baseNodeList.All(n => n.Id != nodeId))
            {
                Debug.WriteLine($"Node not found: {nodeId}");
                errors.Append(missingNodeMessage(portName, nodeId));
                continue;
            }

            Debug.WriteLine($"Node connection valid: {nodeId}");
        }

        if (errors.Length > 0)
        {
            Debug.WriteLine($"Validation errors found: {errors}");
            throw new InvalidOperationException(errors.ToString().Trim());
        }
    }

    private string FormatNode() => $"{Name}:{TypeId}";
}