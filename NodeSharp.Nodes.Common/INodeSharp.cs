using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Common;

public interface INodeSharp
{
    public void Register(IServiceCollection services);
    
    public INodeInformation NodeInformation { get; }
    
    public ContentView? GetNodeBody(BaseNode node);
    
    public string NodeName { get; }
}