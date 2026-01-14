using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Delay.Component;

namespace NodeSharp.Nodes.Delay;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<DelayConfigurePopupComponent, DelayConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }
    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public string NodeName => "Delay";
}