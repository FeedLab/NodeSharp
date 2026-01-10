using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls;
using NodeSharp.Client.Services;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.Common;


public abstract class BaseNode
{
    public event EventHandler<(BaseNode baseNode, string level, string message, string entry)>? OnExitNodeMessage;
    public event EventHandler<BaseNode>? OnEnterNode;
    public event EventHandler<BaseNode>? OnLeaveNode;

    protected readonly IPopupService PopupService;
    
    protected CancellationTokenSource Cts;

    [JsonIgnore] private BaseNodeList Nodes { get; }
    public string Id { get; }
    public string TypeId { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public bool ActivateOnStart { get; }
    public int X { get; set; }
    public int Y { get; set; }
    public IList<Output> Outputs { get; }
    public IList<Input> Inputs { get; }
    
    public ContentView? NodeConfigurePopup { get; set; }

    protected BaseNode(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition,
        int yPosition,
        Storage storage)
    {
        Cts = new CancellationTokenSource();
        PopupService = AppService.GetRequiredService<IPopupService>();

        if (storage.GetNodeInformation().TryGetValue(typeId, out var nodeInformation))
        {
            NodeConfigurePopup = nodeInformation.NodeConfigurePopup;
        }
        else
        {
            throw new InvalidOperationException($"Node type not found: {name}");
        }
        
        if (storage.GetNodeInformation().TryGetValue(typeId, out var nodeType))
        {
            Inputs = new List<Input>();
            Outputs = new List<Output>();

            for(var input = 0 ; input < nodeType.NumberOfInputs ; input++)
            {
                Inputs.Add(new Input("Input 1", new List<string>()));
            }
            
            for(var output = 0 ; output < nodeType.NumberOfOutputs ; output++)
            {
                Outputs.Add(new Output("Output 1", new List<string>()));
            }
        }
        else
        {
            throw new InvalidOperationException($"Node type not found: {typeId}");
        }

        Nodes = nodes;
        Id = id;
        TypeId = typeId;
        Name = name;
        IsEnabled = isEnabled;
        ActivateOnStart = activateOnStart;
        X = xPosition;
        Y = yPosition;
    }

    protected BaseNode(
        BaseNodeList nodes,
        string id,
        string typeId,
        string name,
        bool isEnabled,
        bool activateOnStart,
        int xPosition,
        int yPosition,
        List<Output> outputs,
        List<Input> inputs)
    {
        var storage = AppService.GetRequiredService<Storage>();
        PopupService = AppService.GetRequiredService<IPopupService>();
        
        if (storage.GetNodeInformation().TryGetValue(typeId, out var nodeInformation))
        {
            NodeConfigurePopup = nodeInformation.NodeConfigurePopup;
        }
        else
        {
            throw new InvalidOperationException($"Node type not found: {name}");
        }
        
        Cts = new CancellationTokenSource();
        
        Nodes = nodes;
        Id = id;
        TypeId = typeId;
        Name = name;
        IsEnabled = isEnabled;
        ActivateOnStart = activateOnStart;
        X = xPosition;
        Y = yPosition;
        Outputs = outputs;
        Inputs = inputs;
    }
    
    public void Abort()
    {
        Cts?.Cancel();
    }

    public virtual async Task DisplayNodeConfigurationPopup()
    {
    }
    
    protected virtual void ExitNodeMessage(BaseNode baseNode, string level, string message, string entry)
    {
        OnExitNodeMessage?.Invoke(this, (baseNode, level, message, entry));
    }
    
    protected virtual void EnterNode(BaseNode node)
    {
        OnEnterNode?.Invoke(this, node);
    }
    
    protected virtual void LeaveNode(BaseNode node)
    {
        OnLeaveNode?.Invoke(this, node);
    }

    public virtual Task Run()
    {
        if (!ActivateOnStart)
        {
            return Task.CompletedTask;
        }

        Cts = new CancellationTokenSource();

        Debug.WriteLine($"BaseNode {FormatNode()} has been activated during start of node");
        
        return Task.CompletedTask;
    }

    public virtual Task<string> RunFromInput(BaseNode parent, string parametersJsonString)
    {
        Cts = new CancellationTokenSource();
        
        Debug.WriteLine($"Node {FormatNode()} has been activated by parent node {parent.FormatNode()}");
        return Task.FromResult(parametersJsonString);
    }

    protected Task SendToConnectedChildrenAsync(string parametersJsonString)
    {
        Task.Run(() =>
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

            return Task.FromResult(Task.WhenAll(tasks));
        });
        
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

        Debug.WriteLine("NodeId is valid");
    }

    private void ValidateOutputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Outputs.Count} output connections");

        ValidateConnections(
            baseNodeList,
            connections: Outputs.SelectMany(o =>
                o.ConnectsToNodeId.Select(nodeId => (PortName: o.Name, NodeId: nodeId))),
            idLabel: "Output ConnectsToNodeId",
            missingNodeMessage: (portName, nodeId) =>
                $"Output '{portName}' connects to non-existing node '{nodeId}'. ");
    }

    private void ValidateInputConnections(BaseNodeList baseNodeList)
    {
        Debug.WriteLine($"Validating {Inputs.Count} input connections");

        ValidateConnections(
            baseNodeList,
            connections: Inputs.SelectMany(i =>
                i.ConnectsToParentNodeId.Select(nodeId => (PortName: i.Name, NodeId: nodeId))),
            idLabel: "Input ConnectsToParentNodeId",
            missingNodeMessage: (portName, nodeId) =>
                $"Input '{portName}' connects to non-existing parent node '{nodeId}'. ");
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
    
    protected static JsonElement GetProperty(JsonElement nodeElement, string propertyName)
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

    protected static bool TryGetProperty(JsonElement nodeElement, string propertyName, out JsonElement propertyValue)
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

    private string FormatNode() => $"{Name}:{TypeId}";
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