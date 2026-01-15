using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class BoxNodeStatusGaugeComponent : ContentView
{
    private readonly BoxNodeStatusGaugeViewModel viewModel;

    public BoxNodeStatusGaugeComponent(BaseNode node)
    {
        viewModel = new BoxNodeStatusGaugeViewModel(node);
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}