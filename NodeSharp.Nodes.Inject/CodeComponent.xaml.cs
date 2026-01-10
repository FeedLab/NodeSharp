using NodeSharp.Client.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject;

public partial class CodeComponent : ContentView
{
    public CodeComponent()
    {
        var viewModel1 = AppService.GetRequiredService<InjectViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel1;
    }
}