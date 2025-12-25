using Microsoft.Maui.Layouts;
using NodeSharp.Client.Component;
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
        
        AddBox(50, 50, Colors.Red);
        AddBox(200, 50, Colors.Green);
        AddBox(350, 50, Colors.Blue);
    }
    
    void AddBox(double x, double y, Color color)
    {
        var box = new DraggableBoxComponent
        {
            BoxColor = color,
            WidthRequest = 100,
            HeightRequest = 100
        };

        AbsoluteLayout.SetLayoutBounds(box, new Rect(x, y, 100, 100));
        AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.None);

        Canvas.Children.Add(box);
    }
}