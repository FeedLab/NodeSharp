using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class BoxNodeStatusGaugeComponent : ContentView
{
    public BoxNodeStatusGaugeComponent(BaseNode node)
    {
        var viewModel = new BoxNodeStatusGaugeViewModel(node);
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}