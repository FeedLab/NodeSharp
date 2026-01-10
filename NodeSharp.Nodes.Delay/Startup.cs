using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Delay.Component;

namespace NodeSharp.Nodes.Delay;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<DelayConfigurePopupComponent, DelayConfigurePopupViewModel>();
    }
}