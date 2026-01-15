using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Debug.Component;

public partial class DebugConfigurePopupComponent : ContentView
{
    public DebugConfigurePopupComponent()
    {
        var viewModel1 = AppService.GetRequiredService<ViewModel.DebugConfigurePopupViewModel>();

        InitializeComponent();

        this.BindingContext = viewModel1;
    }
}