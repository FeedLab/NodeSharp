using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Layouts;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class DiagramViewComponent : ContentView
{
    private readonly DiagramViewModel viewModel;

    public DiagramViewComponent()
    {
        viewModel = AppService.GetRequiredService<DiagramViewModel>();
        
        InitializeComponent();

        this.BindingContext = viewModel;
        
        // AddBox(50, 50, Colors.Red);
        // AddBox(200, 50, Colors.Green);
        // AddBox(350, 50, Colors.Blue);
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