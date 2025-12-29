using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client.Component;

public partial class DiagramViewComponent : ContentView
{
    private readonly DiagramViewModel viewModel;
    private readonly CurvedLineDrawable curvedLineDrawable;
    private readonly LineConnectionManager lineConnectionManager;
    private readonly NodeIo nodeIo;

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
        curvedLineDrawable = AppService.GetRequiredService<CurvedLineDrawable>();
        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        nodeIo = AppService.GetRequiredService<NodeIo>();
        
        InitializeComponent();

        // Set Anchors to top-left to make translation math consistent
        CanvasSurface.AnchorX = 0;
        CanvasSurface.AnchorY = 0;

        this.BindingContext = viewModel;

        // In the constructor after setting BindingContext
        viewModel.BoxNodes.CollectionChanged += OnBoxNodesChanged;

        // Add pan gesture to CanvasSurface for canvas panning (empty space)
        var panCanvas = new PanGestureRecognizer();
        panCanvas.PanUpdated += OnCanvasPan;
        CanvasSurface.GestureRecognizers.Add(panCanvas);

        // Also add to ConnectionCanvas for when it's visible
        var panGraphics = new PanGestureRecognizer();
        panGraphics.PanUpdated += OnCanvasPan;
        ConnectionCanvas.GestureRecognizers.Add(panGraphics);

        // Add mouse wheel zoom support for Windows
#if WINDOWS
        this.HandlerChanged += OnHandlerChanged;
#endif

        WeakReferenceMessenger.Default.Register<NodeDraggingStatus>(this, (sender, args) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                DraggingStatus.IsNodeInDraggingMode = args.IsNodeInDraggingMode;
                return Task.CompletedTask;
            }); 
        });
        
        WeakReferenceMessenger.Default.Register<ConnectionPointStatus>(this, (sender, args) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {

                ConnectionCanvas.Invalidate();
                return Task.CompletedTask;
            }); 
        });
        
        this.SizeChanged += (sender, eventArgs) =>
        {
            viewportWidth = this.Width;
            viewportHeight = this.Height;
            ClampPan();
        };

    }
    
    private void OnBoxNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            lineConnectionManager.RecalculateLines(viewModel.BoxNodes);

            // Handle newly added items
            ConnectionCanvas.Invalidate(); // Redraw lines
            
            foreach (var boxNode in viewModel.BoxNodes)
            {
             //   boxNode.OnPropertyChanged(nameof(boxNode.Height));
            }
        }
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

    void OnCanvasPan(object sender, PanUpdatedEventArgs e)
    {
        // If a node is being dragged, don't pan the canvas
        if (DraggingStatus.IsNodeInDraggingMode)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                ConnectionCanvas.Invalidate();
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
                ConnectionCanvas.TranslationX = panX;
                ConnectionCanvas.TranslationY = panY;
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

#if WINDOWS
    void OnHandlerChanged(object sender, EventArgs e)
    {
        if (this.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement nativeView)
        {
            nativeView.PointerWheelChanged += OnNativePointerWheelChanged;
        }
    }

    void OnNativePointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;

        if (delta != 0)
        {
            // Zoom in/out based on wheel direction
            var zoomFactor = delta > 0 ? 1.1 : 0.9;
            scale *= zoomFactor;
            scale = Math.Clamp(scale, 0.3, 3.0);
            UpdateTransform();

            e.Handled = true;
        }
    }
#endif

    void UpdateTransform()
    {
        ClampPan();

        CanvasSurface.AnchorX = 0;
        CanvasSurface.AnchorY = 0;
        CanvasSurface.Scale = scale;
        CanvasSurface.TranslationX = panX;
        CanvasSurface.TranslationY = panY;

        ConnectionCanvas.AnchorX = 0;
        ConnectionCanvas.AnchorY = 0;
        ConnectionCanvas.Scale = scale;
        ConnectionCanvas.TranslationX = panX;
        ConnectionCanvas.TranslationY = panY;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        // Allow drop
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        // Get the dropped data (NodeInformationModel from ListView)
        var data = e.Data.Properties["Data"];

        if (data is NodeInformationModel nodeInfo)
        {
            // Calculate drop position accounting for canvas transformations
            var dropPosition = e.GetPosition(this);
            if (dropPosition != null)
            {
                var dropX = (dropPosition.Value.X - panX) / scale;
                var dropY = (dropPosition.Value.Y - panY) / scale;


                nodeIo.Add(nodeInfo.TypeId, dropX, dropY);

                lineConnectionManager.RecalculateLines(viewModel.BoxNodes);
                
                // Add node to diagram at drop position
                // viewModel.AddNode(nodeInfo, dropX, dropY);
            }
        }
    }

}

