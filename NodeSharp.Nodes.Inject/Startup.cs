using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.Component;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<ParameterEditorPopupComponent, ParameterEditorPopupViewModel>();
        services.AddSingletonPopup<InjectConfigurePopupComponent, InjectConfigurePopupViewModel>();
        services.AddSingleton<InjectConfigurePopupViewModel>();
        services.AddSingleton<NodeInjectParameterViewModel>();
        
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }

    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public string NodeName => "Inject";
}