using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.Options;
using NodeSharp.Nodes.Common.Configuration;
using NodeSharp.Nodes.Common.ViewModels;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Common.Components;

public partial class NodeExplanationsPopup : ContentView
{
    private readonly Color normalColor = Color.FromArgb("#42A5F5");
    private readonly Color hoverColor = Color.FromArgb("#64B5F6");

    public bool UseEditableFields => GetSettings()?.UseEditableFields ?? false;
    public bool UseReadOnlyFields => !UseEditableFields;

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

    private static ExplanationsPopupSettings? GetSettings()
    {
        var options = AppService.GetService<IOptions<ExplanationsPopupSettings>>();
        return options?.Value;
    }

    private void SetupCopyButtonHover()
    {
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, _) =>
        {
            CopyButton.BackgroundColor = hoverColor;
            CopyButton.Scale = 1.05;
        };
        pointerGesture.PointerExited += (s, _) =>
        {
            CopyButton.BackgroundColor = normalColor;
            CopyButton.Scale = 1.0;
        };
        CopyButton.GestureRecognizers.Add(pointerGesture);
    }
}
