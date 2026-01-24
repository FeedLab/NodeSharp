using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class DefaultBoxNodeBodyComponent : ContentView
{
    private readonly IPopupService popupService;
    private readonly DefaultBoxNodeBodyViewModel viewModel;

    public DefaultBoxNodeBodyComponent(BaseNode node)
    {
        viewModel = new DefaultBoxNodeBodyViewModel(node);

        InitializeComponent();

        this.BindingContext = viewModel;

        popupService = AppService.GetRequiredService<IPopupService>();

        SizeChanged += (sender, args) =>
        {
            InvalidateMeasure();
        };
    }

    private async void PointerGestureRecognizer_OnPointerPressed(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (viewModel?.Node is null)
        {
            throw new InvalidOperationException("ViewModel or Node is null");
        }

        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(BaseNode)] = viewModel.Node
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Colors.Transparent
        };

        await popupService.ShowPopupAsync<LastOutputMessageTooltipViewModel>(
            Shell.Current,
            options: popupOptions,
            queryAttributes);
    }
}