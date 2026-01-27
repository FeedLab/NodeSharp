using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace NodeSharp.Nodes.Common.ViewModels;


public partial class ErrorPopupViewModel : ObservableObject, IQueryAttributable
{
    public event EventHandler<bool>? CloseRequested;
    
    [ObservableProperty] private string label;
    [ObservableProperty] private string errorMessage;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Label = query[nameof(Label)] as string ?? string.Empty;
        ErrorMessage = query[nameof(ErrorMessage)] as string ?? string.Empty;
    }
    
    public ErrorPopupViewModel()
    {
        Label = "Error";
        ErrorMessage = "Something went wrong. Please try again.";
    }
    
    [RelayCommand(CanExecute = nameof(CanDoClosePopup))]
    private Task Ok()
    {
        Console.WriteLine("Closing popup!");

        CloseRequested?.Invoke(this, true);

        return Task.CompletedTask;
    }
    
    private bool CanDoClosePopup()
    {
        return true;
    }
}