using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject.Component;

public partial class NodeInjectParameterComponent : ContentView
{
    public NodeInjectParameterComponent()
    {
        var viewModel = AppService.GetRequiredService<NodeInjectParameterViewModel>();
    
        InitializeComponent();
        
        BindingContext = viewModel;
    }
}
