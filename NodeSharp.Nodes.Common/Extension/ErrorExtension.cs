using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Nodes.Common.Extension;

public static class ErrorExtension
{
    private static readonly IPopupService PopupService = AppService.GetRequiredService<IPopupService>();
    
    static ErrorExtension()
    {
    }

    public static async Task ShowPopupAsync(this System.Exception exception, string label = "Error")
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(ViewModels.ErrorPopupViewModel.ErrorMessage)] = exception.Message,
            [nameof(ViewModels.ErrorPopupViewModel.Label)] = label
        };

        await PopupService.ShowPopupAsync<ViewModels.ErrorPopupViewModel>(
            Shell.Current,
            options: null,
            queryAttributes);
    }
}