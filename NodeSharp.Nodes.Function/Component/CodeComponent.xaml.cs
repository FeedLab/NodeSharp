using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.Component;

public partial class CodeComponent : ContentView
{
    private CodeViewModel viewModel;

    public CodeComponent()
    {
        viewModel = AppService.GetRequiredService<CodeViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;

        viewModel.LogMessages.CollectionChanged += (s, e) =>
        {
            if (viewModel.LogMessages.Count > 0)
            {
                LogList.ScrollTo(viewModel.LogMessages.Count - 1, ScrollToPosition.End, true);
            }
        };
    }
}