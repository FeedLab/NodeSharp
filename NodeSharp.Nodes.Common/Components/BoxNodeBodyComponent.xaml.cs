using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class BoxNodeBodyComponent : ContentView
{
    private readonly IPopupService popupService;

    public BoxNodeBodyComponent()
    {
        var viewModel = AppService.GetRequiredService<BoxNodeBodyViewModel>();
    
        InitializeComponent();
     
        this.BindingContext = viewModel;
        
        popupService = AppService.GetRequiredService<IPopupService>();
        
    }
    
    private async void PointerGestureRecognizer_OnPointerPressed(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (BindingContext is BaseNode baseNode)
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(BaseNode)] = baseNode
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
}