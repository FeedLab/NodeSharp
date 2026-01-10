using NodeSharp.Client.Services;

namespace NodeSharp.Nodes.Function.Component;

public partial class FunctionConfigurePopupComponent : ContentView
{
    public FunctionConfigurePopupComponent()
    {
        var viewModel1 = AppService.GetRequiredService<FunctionConfigurePopupViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel1;
    }
}