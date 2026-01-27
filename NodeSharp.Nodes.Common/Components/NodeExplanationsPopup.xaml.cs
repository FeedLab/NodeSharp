using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class NodeExplanationsPopup : ContentView
{
    private readonly Color _normalColor = Color.FromArgb("#42A5F5");
    private readonly Color _hoverColor = Color.FromArgb("#64B5F6");

    public NodeExplanationsPopup()
    {
        InitializeComponent();
        SetupCopyButtonHover();
    }

    public NodeExplanationsPopup(NodeExplanationsPopupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        SetupCopyButtonHover();
    }

    private void SetupCopyButtonHover()
    {
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) =>
        {
            CopyButton.BackgroundColor = _hoverColor;
            CopyButton.Scale = 1.05;
        };
        pointerGesture.PointerExited += (s, e) =>
        {
            CopyButton.BackgroundColor = _normalColor;
            CopyButton.Scale = 1.0;
        };
        CopyButton.GestureRecognizers.Add(pointerGesture);
    }
}
