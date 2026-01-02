using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Client.ViewModel;

public partial class ErrorPopupViewModel : ObservableObject, IQueryAttributable
{
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
}