using System;
using System.Collections.Generic;
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

        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var position = this.GetAbsolutePosition("CanvasSurface");

        if (position.HasValue)
        {
            anchorPoint.CanvasX = position.Value.X;
            anchorPoint.CanvasY = position.Value.Y;
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
        var visualAncestor = this.FindVisualAncestor<AbsoluteLayout>();

        visualAncestor = visualAncestor?.FindVisualAncestor<AbsoluteLayout>();
        
        var position = e.GetPosition(this);
        if (position.HasValue)
        {
            lineConnectionManager.DragEndPoint = new Point(position.Value.X, position.Value.Y);
        }

        lineConnectionManager.EndDragging(anchorPoint);
        WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = false });
        WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = false });  }
}