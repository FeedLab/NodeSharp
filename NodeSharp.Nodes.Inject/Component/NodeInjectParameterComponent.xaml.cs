using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;

namespace NodeSharp.Nodes.Inject.Component;

public partial class NodeInjectParameterComponent : ContentView
{
    private NodeInjectParameterViewModel _viewModel;

    public NodeInjectParameterComponent()
    {
        _viewModel = AppService.GetRequiredService<NodeInjectParameterViewModel>();

        InitializeComponent();

        BindingContext = _viewModel;

        DataGrid.SelectionChanged += (s, e) =>
        {
            _viewModel.SelectedItem = DataGrid.SelectedRow as ParameterItem;
        };
    }
}
