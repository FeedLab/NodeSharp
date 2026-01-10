using NodeSharp.Client.Services;

namespace NodeSharp.Nodes.Delay.Component;

public partial class DelayConfigurePopupComponent : ContentView
{
    private DelayConfigurePopupViewModel viewModel;

    public DelayConfigurePopupComponent()
    {
        viewModel = AppService.GetRequiredService<DelayConfigurePopupViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}