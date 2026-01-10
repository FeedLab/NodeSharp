using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.Component;

public partial class ToolBarComponent : ContentView
{
    private readonly ToolBarViewModel viewModel;

    public ToolBarComponent()
    {
        viewModel = AppService.GetRequiredService<ToolBarViewModel>();
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}