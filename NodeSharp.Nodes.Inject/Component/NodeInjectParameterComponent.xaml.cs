using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Inject.ViewModel;
using Syncfusion.Maui.DataGrid;

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

    private async void DataGrid_OnCellDoubleTapped(object? sender, DataGridCellDoubleTappedEventArgs e)
    {
        if (e.RowData is ParameterItem item)
        {
            await _viewModel.RowDoubleClickCommand.ExecuteAsync(item);
            DataGrid.View?.Refresh();
        }
    }
    
    public void RefreshDataGrid()
    {
        DataGrid.View?.Refresh();
    }
}
