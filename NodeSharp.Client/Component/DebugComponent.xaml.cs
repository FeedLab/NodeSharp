using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class DebugComponent : ContentView
{
    private readonly DebugViewModel viewModel;

    public DebugComponent()
    {
        viewModel = AppService.GetRequiredService<DebugViewModel>();

        InitializeComponent();

        this.BindingContext = viewModel;

        var messages = viewModel.DebugInfo;
        messages?.CollectionChanged += (_, e) =>
        {
            if (messages.Count == 0)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Ensure the UI has actually rendered the new items before scrolling
                    await Task.Delay(100); 

                    if (CollectionViewDebug.ItemsSource != null && messages.Count > 0)
                    {
                        CollectionViewDebug.ScrollTo(
                            index: messages.Count - 1,
                            position: ScrollToPosition.End,
                            animate: true);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Scroll error: {ex.Message}");
                }
            });
        };
    }
}