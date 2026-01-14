using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Function.Component;

public partial class FunctionConfigurePopupComponent : ContentView
{
    public FunctionConfigurePopupComponent()
    {
        var viewModel = AppService.GetRequiredService<FunctionConfigurePopupViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}