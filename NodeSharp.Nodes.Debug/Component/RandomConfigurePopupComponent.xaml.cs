using NodeSharp.Client.Services;
using NodeSharp.Nodes.Random.ViewModel;

namespace NodeSharp.Nodes.Random.Component;

public partial class RandomConfigurePopupComponent : ContentView
{
    private RandomConfigurePopupViewModel viewModel;

    public RandomConfigurePopupComponent()
    {
        viewModel = AppService.GetRequiredService<RandomConfigurePopupViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}