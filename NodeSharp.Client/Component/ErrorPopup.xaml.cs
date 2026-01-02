using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

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