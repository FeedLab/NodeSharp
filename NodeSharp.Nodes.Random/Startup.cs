using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Random.Component;
using NodeSharp.Nodes.Random.ViewModel;

namespace NodeSharp.Nodes.Random;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<RandomConfigurePopupComponent, RandomConfigurePopupViewModel>();
    }
}