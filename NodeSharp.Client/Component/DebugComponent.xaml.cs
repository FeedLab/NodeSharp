using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class DebugComponent : ContentView
{
    private readonly DebugViewModel viewModel;

    public DebugComponent()
    {
        viewModel = AppService.GetRequiredService<DebugViewModel>();

        InitializeComponent();

        this.BindingContext = viewModel;
    }
}