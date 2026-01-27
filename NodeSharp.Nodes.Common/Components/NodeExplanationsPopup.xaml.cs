using Microsoft.Maui.Controls;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class NodeExplanationsPopup : ContentView
{
    public NodeExplanationsPopup()
    {
        InitializeComponent();
    }

    public NodeExplanationsPopup(NodeExplanationsPopupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
