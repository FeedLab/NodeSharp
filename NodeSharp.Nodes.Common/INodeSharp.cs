using Microsoft.Extensions.DependencyInjection;

namespace NodeSharp.Nodes.Common;

public interface INodeSharp
{
    public void Register(IServiceCollection services);
}