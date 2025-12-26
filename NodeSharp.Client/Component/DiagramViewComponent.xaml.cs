using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Layouts;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class DiagramViewComponent : ContentView
{
    private readonly DiagramViewModel viewModel;

    private NodeDraggingStatus DraggingStatus { get; set; } = new();

    double startX = 0;
    double startY = 0;
    double panX, panY;
    double scale = 1.0;

    double viewportWidth, viewportHeight;
    double canvasWidth = 3000;   // virtual size
    double canvasHeight = 2000;
    
    public DiagramViewComponent()
    {
        viewModel = AppService.GetRequiredService<DiagramViewModel>();
        
        InitializeComponent();

        // Set Anchors to top-left to make translation math consistent
        CanvasSurface.AnchorX = 0;
        CanvasSurface.AnchorY = 0;

        this.BindingContext = viewModel;
        
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        CanvasSurface.GestureRecognizers.Add(pan);

        // Also add to Grid to capture events outside canvas
        var panGrid = new PanGestureRecognizer();
        panGrid.PanUpdated += OnPanUpdated;
        this.GestureRecognizers.Add(panGrid);
        
        // var pinch = new PinchGestureRecognizer();
        // pinch.PinchUpdated += OnCanvasPinch;
        // CanvasSurface.GestureRecognizers.Add(pinch);
        //
        // var pinchGrid = new PinchGestureRecognizer();
        // pinchGrid.PinchUpdated += OnCanvasPinch;
        // this.GestureRecognizers.Add(pinchGrid);
        
        WeakReferenceMessenger.Default.Register<NodeDraggingStatus>(this, (sender, args) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                DraggingStatus.IsNodeDragging = args.IsNodeDragging;
                return Task.CompletedTask;
            }); 
        });
        
        this.SizeChanged += (_, __) =>
        {
            viewportWidth = this.Width;
            viewportHeight = this.Height;
            ClampPan();
        };

    }
    
    void ClampPan()
    {
        // Calculate based on the virtual canvas size vs the actual visible area (viewportWidth/Height)
        var scaledCanvasWidth = canvasWidth * scale;
        var scaledCanvasHeight = canvasHeight * scale;

        // Prevent panning too far right (keeping canvas edge at viewport edge)
        double minX = viewportWidth - scaledCanvasWidth;
        double minY = viewportHeight - scaledCanvasHeight;

        // If canvas is smaller than viewport, keep it at 0 or center it
        // If larger, clamp between the negative offset and 0
        panX = scaledCanvasWidth > viewportWidth ? Math.Clamp(panX, minX, 0) : 0;
        panY = scaledCanvasHeight > viewportHeight ? Math.Clamp(panY, minY, 0) : 0;
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Pan event: {e.StatusType}, IsNodeDragging: {DraggingStatus.IsNodeDragging}");

        if (DraggingStatus.IsNodeDragging)
        {
            Element? current = sender as Element;
            while (current != null && current is not DraggableBoxComponent)
            {
                current = current.Parent;
            }

            if (current is DraggableBoxComponent element)
            {
                if (e.StatusType == GestureStatus.Started)
                {
                    // 1. Set ZIndex (as a backup)
                    element.ZIndex = 1000;

                    // 2. Move the data item to the end of the collection to force it to the top of the visual stack
                    if (element.BindingContext is BoxNode boxNode)
                    {
                        viewModel.MoveNodeToFront(boxNode);
                    }
                }
                else if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
                {
                    element.ZIndex = 1;
                }
            }
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                System.Diagnostics.Debug.WriteLine("Canvas pan ACTUALLY started");
                startX = panX;
                startY = panY;
                break;

            case GestureStatus.Running:
                panX = startX + e.TotalX;
                panY = startY + e.TotalY;

                System.Diagnostics.Debug.WriteLine($"Before clamp: panX={panX}, panY={panY}, viewport={viewportWidth}x{viewportHeight}, canvas=3000x2000");
                ClampPan();
                System.Diagnostics.Debug.WriteLine($"After clamp: panX={panX}, panY={panY}");

                CanvasSurface.TranslationX = panX;
                CanvasSurface.TranslationY = panY;
                break;

            case GestureStatus.Completed:
                System.Diagnostics.Debug.WriteLine("Canvas pan ACTUALLY completed");
                ClampPan();
                UpdateTransform();
                break;
        }
    }
    
    void OnCanvasPinch(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Running)
        {
            scale *= e.Scale;
            scale = Math.Clamp(scale, 0.3, 3.0);
            UpdateTransform();
        }
    }

    
    void UpdateTransform()
    {
        ClampPan();

        CanvasSurface.AnchorX = 0;
        CanvasSurface.AnchorY = 0;
        CanvasSurface.Scale = scale;
        CanvasSurface.TranslationX = panX;
        CanvasSurface.TranslationY = panY;
    }

    
    
}

public class NodeDraggingStatus
{
    public bool IsNodeDragging { get; set; }
}