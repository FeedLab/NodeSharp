using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Delay.Component;

public partial class DelayConfigurePopupComponent : ContentView
{
    public DelayConfigurePopupComponent()
    {
        var viewModel = AppService.GetRequiredService<DelayConfigurePopupViewModel>();

        InitializeComponent();

        this.BindingContext = viewModel;
    }
}