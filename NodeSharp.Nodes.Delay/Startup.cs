using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
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

    public ContentView? GetNodeBody(BaseNode node)
    {
        return new DefaultBoxNodeBodyComponent(node);
    }
    
    public ContentView? GetNBoxNodeStatusComponent(BaseNode node)
    {
        return new BoxNodeStatusGaugeComponent(node);
    }
    
    public string NodeName => "Delay";
    
    public Type NodeType => typeof(NodeDelay);
}