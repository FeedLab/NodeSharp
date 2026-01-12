using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject.Component;

public partial class ParameterEditorPopupComponent : ContentView
{
    public ParameterEditorPopupComponent()
    {
        var viewModel = AppService.GetRequiredService<ParameterEditorPopupViewModel>();
        
        InitializeComponent();

        this.BindingContext = viewModel;
    }
}
