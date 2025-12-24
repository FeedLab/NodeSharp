using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace ConsoleApp1;

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

        Debug.WriteLine($"Node {Name}:{TypeId} is activated on start");

        return Task.CompletedTask;
    }

    protected virtual Task RunFromInput(BaseNode parent, string parametersJsonString)
    {
        Debug.WriteLine($"Node {Name}:{TypeId} has been activate by parent node {parent.Name}:{parent.TypeId}");

        return Task.CompletedTask;
    }

    protected Task SendToChildren(string parametersJsonString)
    {
        foreach (var output in Outputs)
        {
            foreach (var nodeId in output.ConnectsToNodeId)
            {
                var baseNode = Nodes.Find(f => f.Id == nodeId);

                if (baseNode is null)
                {
                    throw new InvalidOperationException($"Node not found: {nodeId}");
                }

                Task.Run(() => baseNode.RunFromInput(this, parametersJsonString));
            }
        }
        
        return Task.CompletedTask;
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

        Debug.WriteLine($"NodeId is valid");
    }

    private void ValidateOutputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Outputs.Length} output connections");
        var errors = new StringBuilder();

        foreach (var output in Outputs)
        {
            Debug.WriteLine($"Validating output: {output.Name}");

            foreach (var nodeId in output.ConnectsToNodeId)
            {
                Debug.WriteLine($"Checking node connection: {nodeId}");

                if (!Guid.TryParse(nodeId, out _))
                {
                    Debug.WriteLine($"Invalid GUID: {nodeId}");
                    errors.Append($"Output ConnectsToNodeId '{nodeId}' is not a valid GUID. ");
                }
                else if (baseNodeList.All(n => n.Id != nodeId))
                {
                    Debug.WriteLine($"Node not found: {nodeId}");
                    errors.Append($"Output '{output.Name}' connects to non-existing node '{nodeId}'. ");
                }
                else
                {
                    Debug.WriteLine($"Node connection valid: {nodeId}");
                }
            }
        }

        if (errors.Length > 0)
        {
            Debug.WriteLine($"Validation errors found: {errors}");
            throw new InvalidOperationException(errors.ToString().Trim());
        }
    }
    
    private void ValidateInputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Inputs.Length} input connections");
        var errors = new StringBuilder();

        foreach (var input in Inputs)
        {
            Debug.WriteLine($"Validating input: {input.Name}");

            foreach (var nodeId in input.ConnectsToParentNodeId)
            {
                Debug.WriteLine($"Checking parent node connection: {nodeId}");

                if (!Guid.TryParse(nodeId, out _))
                {
                    Debug.WriteLine($"Invalid GUID: {nodeId}");
                    errors.Append($"Input ConnectsToParentNodeId '{nodeId}' is not a valid GUID. ");
                }
                else if (baseNodeList.All(n => n.Id != nodeId))
                {
                    Debug.WriteLine($"Parent node not found: {nodeId}");
                    errors.Append($"Input '{input.Name}' connects to non-existing parent node '{nodeId}'. ");
                }
                else
                {
                    Debug.WriteLine($"Parent node connection valid: {nodeId}");
                }
            }
        }

        if (errors.Length > 0)
        {
            Debug.WriteLine($"Validation errors found: {errors}");
            throw new InvalidOperationException(errors.ToString().Trim());
        }
    }
    


}