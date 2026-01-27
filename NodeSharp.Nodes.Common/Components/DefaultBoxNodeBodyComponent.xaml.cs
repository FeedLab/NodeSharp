using System.Collections.Generic;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.Model;
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