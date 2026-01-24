using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.KS0212.Component;
using NodeSharp.Nodes.KS0212.ViewModel;

namespace NodeSharp.Nodes.KS0212;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<Ks0212ConfigurePopupComponent, Ks0212ConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }

    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public ContentView? GetNodeBody(BaseNode node)
    {
        return new NodeBodyComponent(node);
    }
    
    public ContentView? GetNBoxNodeStatusComponent(BaseNode node)
    {
        return new MultiBoxComponent(4, Colors.Green, Colors.Red);
    }
    
    public string NodeName => "KS0212";
    
    public Type NodeType => typeof(NodeKs0212);
}