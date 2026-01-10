using NodeSharp.Nodes.Common;

namespace NodeSharp.Nodes.Debug;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<CodeComponent, InjectViewModel>();
    }
}