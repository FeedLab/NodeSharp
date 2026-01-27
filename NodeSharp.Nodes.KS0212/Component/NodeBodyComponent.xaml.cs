using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
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
    
    private void OnSymbolPointerEntered(object? sender, PointerEventArgs e)
    {
        if (viewModel?.Node?.Explanations.Count > 0)
        {
            SymbolLabel.BackgroundColor = Colors.LightBlue;
            SymbolLabel.Opacity = 0.8;
        }
    }

    private void OnSymbolPointerExited(object? sender, PointerEventArgs e)
    {
        SymbolLabel.BackgroundColor = Colors.Transparent;
        SymbolLabel.Opacity = 1.0;
    }
    
    private async void OnSymbolDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (viewModel?.Node?.Explanations.Count > 0)
        {
            var baseNode = viewModel.Node;
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(BaseNode)] = baseNode
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            };
            
            await popupService.ShowPopupAsync<NodeExplanationsPopupViewModel>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes);
            
            WeakReferenceMessenger.Default.Send(new ShowExplanationsMessage(viewModel.Node));
        }
    }
}