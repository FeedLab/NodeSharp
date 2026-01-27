using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Common;

public interface INodeSharp
{
    public void Register(IServiceCollection services);
    
    public INodeInformation NodeInformation { get; }
    
    public ContentView? GetNodeBody(BaseNode node);
    
    public ContentView? GetNBoxNodeStatusComponent(BaseNode node);
    
    public string NodeName { get; }
    
    public Type NodeType { get; }
}