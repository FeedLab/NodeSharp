using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Random.Component;
using NodeSharp.Nodes.Random.ViewModel;

namespace NodeSharp.Nodes.Random;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<RandomConfigurePopupComponent, RandomConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
    }

    public INodeInformation NodeInformation => AppService.GetRequiredKeyedService<INodeInformation>(NodeName);

    public ContentView? GetNodeBody(BaseNode node)
    {
        return new DefaultBoxNodeBodyComponent(node);
    }
    
    public ContentView? GetNBoxNodeStatusComponent(BaseNode node)
    {
        return new BoxNodeStatusTextComponent(node);
    }
    
    public string NodeName => "RandomNumber";
    
    public Type NodeType => typeof(NodeRandomNumber);
}
