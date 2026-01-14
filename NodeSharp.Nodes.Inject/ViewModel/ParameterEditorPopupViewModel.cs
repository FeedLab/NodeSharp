using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Inject.ViewModel;

public partial class ParameterEditorPopupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] private string name = "<Change this>";

    [ObservableProperty] private string primitiveType = "String";

    [ObservableProperty] private string primitiveValue = string.Empty;

    [ObservableProperty] private int selectedTabIndex;

    [ObservableProperty] private ParameterItem originalParameter;

    private readonly IPopupService popupService;

    public bool IsNameValid => !string.IsNullOrWhiteSpace(Name);

    public ParameterItem? ParameterItem { get; set; }

    public ParameterEditorPopupViewModel(IPopupService popupService)
    {
        this.popupService = popupService;

        SelectedTabIndex = 0;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task OnOk()
    {
        if (IsNameValid && ParameterItem is not null)
        {
            ParameterItem.Name = Name;
            ParameterItem.Type = PrimitiveType;
            ParameterItem.Value = PrimitiveValue;
            ParameterItem.Source = SelectedTabIndex switch
            {
                0 => "Primitive",
                1 => "Environment",
                2 => "Timestamp",
                _ => throw new InvalidOperationException("Invalid tab index")
            };

            // Copy values back to original parameter
            OriginalParameter.Name = ParameterItem.Name;
            OriginalParameter.Source = ParameterItem.Source;
            OriginalParameter.Type = ParameterItem.Type;
            OriginalParameter.Value = ParameterItem.Value;
            
            await popupService.ClosePopupAsync(Shell.Current, true);
        }
    }

    bool CanSave()
    {
        return IsNameValid;
    }

    [RelayCommand]
    private async Task OnCancel()
    {
        await popupService.ClosePopupAsync(Shell.Current, false);
    }

    partial void OnNameChanged(string value)
    {
        CancelCommand.NotifyCanExecuteChanged();
        OkCommand.NotifyCanExecuteChanged();
    }

    partial void OnPrimitiveTypeChanged(string value)
    {
        CancelCommand.NotifyCanExecuteChanged();
        OkCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        CancelCommand.NotifyCanExecuteChanged();
        OkCommand.NotifyCanExecuteChanged();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        OriginalParameter = (ParameterItem)query[nameof(ParameterItem)];
        
        ParameterItem = OriginalParameter.Copy();

        Name = ParameterItem.Name;
        PrimitiveValue = ParameterItem.Value;
        PrimitiveType = ParameterItem.Type;
        SelectedTabIndex = ParameterItem.Source switch
        {
            "Primitive" => 0,
            "Environment" => 1,
            "Timestamp" => 2,
            _ => throw new InvalidOperationException("Invalid source")
        };
    }
}