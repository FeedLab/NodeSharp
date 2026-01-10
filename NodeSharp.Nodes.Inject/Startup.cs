using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<InjectConfigurePopupComponent, InjectViewModel>();
        services.AddSingleton<InjectViewModel>();
    }
}