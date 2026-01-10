using NodeSharp.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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