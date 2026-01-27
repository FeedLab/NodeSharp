using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Common.ViewModels;

public partial class NodeExplanationsPopupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private ObservableCollection<ExplanationItem> explanations = new();

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

    public NodeExplanationsPopupViewModel()
    {
        UpdateCurrentItem();
    }

    public void Initialize(ObservableCollection<ExplanationItem> items)
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
        // Close popup - will be handled by the popup service
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var baseNode = (BaseNode)query[nameof(BaseNode)];

        Initialize(baseNode.Explanations);
    }
}
