using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ConsoleApp1;

public class NodeDebug : BaseNode
{
    // public NodeDebug(
    //     BaseNodeList nodes,
    //     string id,
    //     string typeId,
    //     string name,
    //     bool isEnabled,
    //     bool activateOnStart,
    //     Output[] outputs,
    //     Input[] inputs)
    //     : base(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs)
    // {
    // }

    public NodeDebug(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        Output[] outputs,
        Input[] inputs,
        JsonElement nodeElement)
        : base(nodes, id, typeId, name, isEnabled, activateOnStart, outputs, inputs)
    {
    }

    protected override Task RunFromInput(BaseNode parentNode, string parametersJsonString)
    {
        base.RunFromInput(parentNode, parametersJsonString);
        
        Debug.WriteLine($"{Name}: {parametersJsonString}");
        
        return Task.CompletedTask;
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

    private void ValidateInputConnections()
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
                else if (Nodes.All(n => n.Id != nodeId))
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
