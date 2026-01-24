using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class BoxNodeStatusTextComponent : ContentView
{
    public BoxNodeStatusTextComponent(BaseNode node)
    {
        var viewModel = new BoxNodeStatusTextViewModel(node);
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}