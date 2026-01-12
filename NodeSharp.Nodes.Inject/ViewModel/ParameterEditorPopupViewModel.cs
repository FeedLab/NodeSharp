using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Inject.ViewModel;

public partial class ParameterEditorPopupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string primitiveType = "String";

    [ObservableProperty]
    private string primitiveValue = string.Empty;
  
    private readonly IPopupService popupService;

    public bool IsNameValid => !string.IsNullOrWhiteSpace(Name);
    
    public ParameterItem? ParameterItem { get; set; }

    public ParameterEditorPopupViewModel(IPopupService popupService)
    {
        this.popupService = popupService;
    }

    [RelayCommand]
    private Task OnOk()
    {
        if (!IsNameValid)
            return Task.CompletedTask;
        return Task.CompletedTask;

        // Handle OK logic here
        // You can access Name, PrimitiveType, PrimitiveValue properties
    }

    [RelayCommand]
    private async Task OnCancel()
    {
        await popupService.ClosePopupAsync(Shell.Current);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ParameterItem = (ParameterItem)query[nameof(ParameterItem)];
    }

}
