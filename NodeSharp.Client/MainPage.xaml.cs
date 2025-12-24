using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client;

public partial class MainPage : ContentPage
{
    private readonly MainPageModel viewModel;

    public MainPage()
    {
        viewModel = AppService.GetRequiredService<MainPageModel>();
        
        InitializeComponent();
        
        this.BindingContext = viewModel;
        
        viewModel.Init();
    }
}