using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Common.ViewModels;

public partial class NodeExplanationsPopupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private ObservableCollection<ExplanationItem> explanations = [];

    [ObservableProperty]
    private int currentIndex;

    [ObservableProperty]
    private string currentLabel = string.Empty;

    [ObservableProperty]
    private string currentDescription = string.Empty;

    [ObservableProperty]
    private string currentCode = string.Empty;

    [ObservableProperty]
    private bool canGoNext;

    [ObservableProperty]
    private bool canGoPrevious;

    private readonly IPopupService popupService;

    public NodeExplanationsPopupViewModel()
    {
        popupService = AppService.GetRequiredService<IPopupService>();
        
        UpdateCurrentItem();
    }

    private void Initialize(ObservableCollection<ExplanationItem> items)
    {
        Explanations = items;
        CurrentIndex = 0;
        UpdateCurrentItem();
    }

    [RelayCommand]
    private void Next()
    {
        if (CurrentIndex < Explanations.Count - 1)
        {
            CurrentIndex++;
            UpdateCurrentItem();
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            UpdateCurrentItem();
        }
    }

    [RelayCommand]
    private void Close()
    {
        popupService.ClosePopupAsync(Shell.Current);
    }

    [RelayCommand]
    private async Task CopyCode()
    {
        await Clipboard.SetTextAsync(CurrentCode);
    }

    private void UpdateCurrentItem()
    {
        if (Explanations.Count == 0)
        {
            CurrentLabel = string.Empty;
            CurrentDescription = string.Empty;
            CurrentCode = string.Empty;
            CanGoNext = false;
            CanGoPrevious = false;
            return;
        }

        var item = Explanations[CurrentIndex];
        CurrentLabel = item.Label;
        CurrentDescription = item.Description;
        CurrentCode = item.Code;

        CanGoNext = CurrentIndex < Explanations.Count - 1;
        CanGoPrevious = CurrentIndex > 0;
    }

    partial void OnCurrentCodeChanged(string value)
    {
        if (CurrentIndex < 0 || CurrentIndex >= Explanations.Count)
        {
            return;
        }

        Explanations[CurrentIndex].Code = value;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var baseNode = (BaseNode)query[nameof(BaseNode)];

        Initialize(baseNode.Explanations);
    }
}
