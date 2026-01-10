using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Function.Component;

namespace NodeSharp.Nodes.Function;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<FunctionConfigurePopupComponent, FunctionConfigurePopupViewModel>();
    }
}