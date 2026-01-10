using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Debug.Component;

public partial class DebugConfigurePopupComponent : ContentView
{
    public DebugConfigurePopupComponent()
    {
        var viewModel1 = AppService.GetRequiredService<DebugConfigurePopupViewModel>();

        InitializeComponent();

        this.BindingContext = viewModel1;
    }
}