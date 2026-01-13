using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Function.Component;

namespace NodeSharp.Nodes.Function;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<FunctionConfigurePopupComponent, FunctionConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }
    
    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public string NodeName => "Function";
}