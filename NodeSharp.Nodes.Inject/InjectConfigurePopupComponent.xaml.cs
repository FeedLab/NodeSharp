using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject;

public partial class InjectConfigurePopupComponent : ContentView
{
    public InjectConfigurePopupComponent()
    {
        var viewModel1 = AppService.GetRequiredService<InjectViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel1;
    }
}