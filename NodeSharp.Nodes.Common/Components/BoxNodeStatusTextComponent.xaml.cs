using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class BoxNodeStatusTextComponent : ContentView
{
    private readonly BoxNodeStatusTextViewModel viewModel;

    public BoxNodeStatusTextComponent(BaseNode node)
    {
        viewModel = new BoxNodeStatusTextViewModel(node);
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}