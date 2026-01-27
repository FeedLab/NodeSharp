using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Inject.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class ParameterEditorPopupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] private string name;

    [ObservableProperty] private string primitiveType;

    [ObservableProperty] private string primitiveValue;

    [ObservableProperty] private int selectedTabIndex;

    [ObservableProperty] private ParameterItem? originalParameter;

    private readonly IPopupService popupService;

    private bool IsNameValid => !string.IsNullOrWhiteSpace(Name);

    public ParameterItem? ParameterItem { get; set; }

    public ParameterEditorPopupViewModel(IPopupService popupService)
    {
        this.popupService = popupService;
        Name = "<Change this>";
        PrimitiveType = "String";
        PrimitiveValue = string.Empty;

        SelectedTabIndex = 0;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task OnOk()
    {
                await popupService.ClosePopupAsync(Shell.Current, true);
        
        // if (IsNameValid && ParameterItem is not null)
        // {
        //     ParameterItem.Name = Name;
        //     ParameterItem.Type = PrimitiveType;
        //     ParameterItem.Value = PrimitiveValue;
        //     ParameterItem.Source = SelectedTabIndex switch
        //     {
        //         0 => "Primitive",
        //         1 => "Environment",
        //         2 => "Timestamp",
        //         _ => throw new InvalidOperationException("Invalid tab index")
        //     };
        //     
        //     // Copy values back to original parameter
        //     OriginalParameter.Name = ParameterItem.Name;
        //     OriginalParameter.Source = ParameterItem.Source;
        //     OriginalParameter.Type = ParameterItem.Type;
        //     OriginalParameter.Value = ParameterItem.Value;
        //
        //     try
        //     {
        //         await popupService.ClosePopupAsync(Shell.Current, true);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw;
        //     }
        // }
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