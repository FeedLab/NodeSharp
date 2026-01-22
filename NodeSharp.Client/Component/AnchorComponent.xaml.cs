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

    public AnchorComponent(AnchorPoint anchorPoint)
    {
        this.anchorPoint = anchorPoint;
        this.node = this.anchorPoint.BoxNode;

        InitializeComponent();

        // Set column positions based on connection type
        if (anchorPoint.ConnectionType == InOrOutConnection.Out)
        {
            Grid.SetColumn(ConnectionLine, 0);
            Grid.SetColumn(AnchorConnectionControl, 1);
            
            Grid.SetColumn(ConnectionName, 0);
            ConnectionName.Text = anchorPoint.OriginalOutput?.Name ?? string.Empty;
        }
        else
        {
            Grid.SetColumn(ConnectionLine, 1);
            Grid.SetColumn(AnchorConnectionControl, 0);
          
            Grid.SetColumn(ConnectionName, 1);
            ConnectionName.Text = anchorPoint.OriginalInput?.Name ?? string.Empty;
        }
        

        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        var draggableBoxComponent = this.FindVisualAncestor<DraggableBoxComponent>();

        if (draggableBoxComponent != null)
        {
            draggableBoxComponent.PropertyChanged -= OnDraggableBoxComponentOnPropertyChanged;
        }
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
        try
        {
            if (this.Handler == null || !this.IsLoaded) return;
            
            var canvas = this.GetVisualAncestors().OfType<AbsoluteLayout>()
                .Single(s => s.AutomationId == "CanvasSurface");

            if (canvas is null)
            {
                throw new InvalidOperationException("CanvasSurface not found for anchor component");
            }

            var positionPt = AnchorConnectionControl.GetRelativePosition(canvas);

        
            anchorPoint.SetRelativePosition(positionPt, AnchorConnectionControl.Width, AnchorConnectionControl.Height);
        }
        catch (Exception _)
        {
            return;
        }
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