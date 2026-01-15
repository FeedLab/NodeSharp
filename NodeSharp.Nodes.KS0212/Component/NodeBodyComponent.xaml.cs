using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;
using NodeSharp.Nodes.KS0212.ViewModel;

namespace NodeSharp.Nodes.KS0212.Component;

public partial class NodeBodyComponent : ContentView
{
    private readonly NodeBodyViewModel viewModel;
    private readonly IPopupService popupService;

    public NodeBodyComponent(BaseNode node)
    {
        viewModel = new NodeBodyViewModel(node);
     
        InitializeComponent();

        this.BindingContext = viewModel;
        
        popupService = AppService.GetRequiredService<IPopupService>();
    }
    
    private async void PointerGestureRecognizer_OnPointerPressed(object? _, TappedEventArgs tappedEventArgs)
    {
        if (viewModel.Node is null)
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