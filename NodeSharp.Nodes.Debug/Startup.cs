using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Debug.Component;

namespace NodeSharp.Nodes.Debug;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<DebugConfigurePopupComponent, DebugConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }
    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public string NodeName => "Debug";
}