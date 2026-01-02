using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class ErrorPopup : ContentView
{
    public ErrorPopup()
    {
        var viewModel = AppService.GetRequiredService<ErrorPopupViewModel>();
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
    }
}