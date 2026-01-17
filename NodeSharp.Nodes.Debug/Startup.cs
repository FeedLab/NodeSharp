using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Debug.Component;

namespace NodeSharp.Nodes.Debug;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<DebugConfigurePopupComponent, ViewModel.DebugConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }
    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public ContentView? GetNodeBody(BaseNode node)
    {
        return new DefaultBoxNodeBodyComponent(node);
    }
    
    public ContentView? GetNBoxNodeStatusComponent(BaseNode node)
    {
        return null;
    }
    
    public string NodeName => "Debug";
}