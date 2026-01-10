using NodeSharp.Client.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject;

public partial class CodeComponent : ContentView
{
    private InjectViewModel viewModel;

    public CodeComponent()
    {
        viewModel = AppService.GetRequiredService<InjectViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}