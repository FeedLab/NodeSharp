using System.Threading;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class ErrorPopup : ContentView
{
    private readonly IPopupService popupService;

    public ErrorPopup()
    {
        var viewModel = AppService.GetRequiredService<ErrorPopupViewModel>();
        popupService = AppService.GetRequiredService<IPopupService>();
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
        
        viewModel.CloseRequested += (_, result) => ClosePopup(result);
    }

    private void ClosePopup(bool result)
    {
        var page = Shell.Current.CurrentPage;
        popupService.ClosePopupAsync(page, CancellationToken.None);
    }
}