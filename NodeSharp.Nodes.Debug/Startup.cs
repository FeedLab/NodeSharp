using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Debug.Component;

namespace NodeSharp.Nodes.Debug;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<DebugConfigurePopupComponent, DebugConfigurePopupViewModel>();
    }
}