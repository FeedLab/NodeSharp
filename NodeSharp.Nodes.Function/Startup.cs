using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Function.Component;

namespace NodeSharp.Nodes.Function;

public class Startup : INodeSharp
{
    public void Register(IServiceCollection services)
    {
        services.AddTransientPopup<FunctionConfigurePopupComponent, FunctionConfigurePopupViewModel>();
        services.AddKeyedSingleton<INodeInformation, NodeInformation>(NodeName);
        // services.AddSingletonPopup<CodeComponent, CodeViewModel>();
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

    public string NodeName => "Function";
    
    public Type NodeType => typeof(NodeFunction);
}