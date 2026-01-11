using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject.Component;

public partial class InjectConfigurePopupComponent : ContentView
{
    public InjectConfigurePopupComponent()
    {
        var viewModel = AppService.GetRequiredService<InjectViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}