using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client;

public partial class MainPage : ContentPage
{
    private readonly MainPageModel viewModel;



    public MainPage()
    {
        viewModel = AppService.GetRequiredService<MainPageModel>();
        var storage = AppService.GetRequiredService<Storage>();
        
        storage.Initialize();

        InitializeComponent();
        
        this.BindingContext = viewModel;
        
        viewModel.Init();
        
        var diagramViewModelModel = AppService.GetRequiredService<DiagramViewModel>();

        // var pan = new PanGestureRecognizer();
        // pan.PanUpdated += OnPanUpdated;
        // Viewport.GestureRecognizers.Add(pan);
    }
    
    // void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    // {
    //     switch (e.StatusType)
    //     {
    //         case GestureStatus.Started:
    //             startX = panX;
    //             startY = panY;
    //             break;
    //
    //         case GestureStatus.Running:
    //             // Use TotalX and TotalY
    //             panX = startX + e.TotalX;
    //             panY = startY + e.TotalY;
    //             UpdateTransform();
    //             break;
    //
    //         case GestureStatus.Completed:
    //             // Gesture finished
    //             break;
    //     }
    // }


    // void UpdateTransform()
    // {
    //     Viewport.Scale = scale;
    //     Viewport.TranslationX = panX;
    //     Viewport.TranslationY = panY;
    // }

}