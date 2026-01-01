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

            MainThread.BeginInvokeOnMainThread(async void () =>
            {
                try
                {
                    await Task.Yield();

                    CollectionViewDebug.ScrollTo(
                        index: messages.Count - 1,
                        position: ScrollToPosition.End,
                        animate: true);
                }
                catch (Exception e)
                {
                    throw; // TODO handle exception
                }
            });
        };
    }
}