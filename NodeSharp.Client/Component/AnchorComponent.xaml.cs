using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Shapes;
using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.Component;

public partial class AnchorComponent : ContentView
{
    private readonly AnchorPoint anchorPoint;
    private readonly BoxNode node;

    private readonly LineConnectionManager lineConnectionManager;

    private Ellipse AnchorConnectionControl;

    public AnchorComponent(AnchorPoint anchorPoint)
    {
        this.anchorPoint = anchorPoint;
        this.node = this.anchorPoint.BoxNode;

        BuildUI();

        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();

        this.Loaded += OnLoaded;
    }

    private void BuildUI()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(50) },
                new ColumnDefinition { Width = new GridLength(10) }
            }
        };

        var boxView = new BoxView
        {
            HorizontalOptions = LayoutOptions.Fill,
            HeightRequest = 1,
            Color = Colors.LightBlue,
            InputTransparent = false,
            AnchorY = 0.5,
            AnchorX = 0.0
        };

        AnchorConnectionControl = new Ellipse
        {
            WidthRequest = 10,
            HeightRequest = 10,
            Stroke = new SolidColorBrush(Colors.Blue),
            Fill = new SolidColorBrush(Colors.Transparent),
            StrokeThickness = 3,
            InputTransparent = false,
            AnchorY = 0.5,
            AnchorX = 0.5,
            ZIndex = 900
        };

        if (anchorPoint.ConnectionType == InOrOutConnection.Out)
        {
            Grid.SetColumn(boxView, 0);
            Grid.SetColumn(AnchorConnectionControl, 1);
        }
        else
        {
            Grid.SetColumn(boxView, 1);
            Grid.SetColumn(AnchorConnectionControl, 0); 
        }

        var pointerGestureRecognizer = new PointerGestureRecognizer();
        pointerGestureRecognizer.PointerEntered += OnPointerEntered;
        pointerGestureRecognizer.PointerExited += OnPointerExited;
        pointerGestureRecognizer.PointerPressed += OnPointerPressed;
        pointerGestureRecognizer.PointerReleased += OnPointerReleased;
        AnchorConnectionControl.GestureRecognizers.Add(pointerGestureRecognizer);

        grid.Children.Add(boxView);
        grid.Children.Add(AnchorConnectionControl);

        Content = grid;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var draggableBoxComponent = this.FindVisualAncestor<DraggableBoxComponent>();

        if (draggableBoxComponent != null)
        {
            draggableBoxComponent.PropertyChanged += OnDraggableBoxComponentOnPropertyChanged;
        }

        CalculateAbsolutePosition();

        
        // if (position.HasValue)
        // {
        //     anchorPoint.CanvasX = position.Value.X;
        //     anchorPoint.CanvasY = position.Value.Y;
        // }

        
        
    }

    private void OnDraggableBoxComponentOnPropertyChanged(object? s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DraggableBoxComponent.X) or nameof(DraggableBoxComponent.Y))
        {
            CalculateAbsolutePosition();
        }
    }

    private void CalculateAbsolutePosition()
    {
        var canvas = this.GetVisualAncestors().OfType<AbsoluteLayout>()
            .Single(s => s.AutomationId == "CanvasSurface");

        if (canvas is null)
        {
            throw new InvalidOperationException("CanvasSurface not found for anchor component");
        }

        var positionPt = AnchorConnectionControl.GetRelativePosition(canvas);
        positionPt.X += AnchorConnectionControl.Width/2;
        positionPt.Y += AnchorConnectionControl.Height/2;
        
        anchorPoint.RelativePosition = positionPt;
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Ellipse ellipse)
        {
            ellipse.Scale = 1.5;
            ellipse.Stroke = Colors.Red;
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Ellipse ellipse)
        {
            ellipse.Scale = 1.0;
            ellipse.Stroke = Colors.Blue;
        }
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        var positionTmp = this.GetAbsolutePosition("CanvasSurface");
        var visualAncestor = this.FindVisualAncestor<AbsoluteLayout>();

        visualAncestor = visualAncestor?.FindVisualAncestor<AbsoluteLayout>();


        var position = e.GetPosition(visualAncestor);
        if (position.HasValue)
        {
            lineConnectionManager.StartDragging(anchorPoint);
        }

        WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = true });
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        var visualAncestor = this.FindVisualAncestor<DiagramViewComponent>();

       // visualAncestor = visualAncestor?.FindVisualAncestor<AbsoluteLayout>();

        var position = e.GetPosition(visualAncestor);
        if (position.HasValue)
        {
            lineConnectionManager.DragEndPoint = new Point(position.Value.X, position.Value.Y);
        }

        lineConnectionManager.EndDragging(anchorPoint);
        WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = false });
        WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = false });
    }
}