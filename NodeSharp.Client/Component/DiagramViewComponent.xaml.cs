using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Configuration;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using MauiPointerEventArgs = Microsoft.Maui.Controls.PointerEventArgs;
using MauiTappedEventArgs = Microsoft.Maui.Controls.TappedEventArgs;
#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
#endif

namespace NodeSharp.Client.Component;

public partial class DiagramViewComponent : ContentView
{
    private readonly DiagramViewModel viewModel;
    private readonly CurvedLineDrawable curvedLineDrawable;
    private readonly LineConnectionManager lineConnectionManager;
    private readonly NodeIo nodeIo;
    private readonly GridSettings gridSettings;

    private NodeDraggingStatus DraggingStatus { get; set; } = new();
    private AnchorDraggingStatus AnchorDragging { get; set; } = new();

    private double startX = 0;
    private double startY = 0;
    private double panX, panY;
    private double scale = 1.0;

    private double viewportWidth, viewportHeight;
    private const double CanvasWidth = 3000; // virtual size
    private const double CanvasHeight = 2000;
    private const double MarqueeMinDistance = 4;

    private bool isMarqueeSelecting;
    private Point marqueeStartView;
    private Point marqueeStartCanvas;
    private Point marqueeCurrentCanvas;
#if WINDOWS
    private Microsoft.UI.Xaml.FrameworkElement? nativeElement;
    private bool deleteAcceleratorAttached;
#endif

    public DiagramViewComponent()
    {
        viewModel = AppService.GetRequiredService<DiagramViewModel>();
        curvedLineDrawable = AppService.GetRequiredService<CurvedLineDrawable>();
        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        nodeIo = AppService.GetRequiredService<NodeIo>();
        gridSettings = AppService.GetRequiredService<Microsoft.Extensions.Options.IOptions<GridSettings>>().Value;

        InitializeComponent();

        // Set Anchors to top-left to make translation math consistent
        CanvasSurface.AnchorX = 0;
        CanvasSurface.AnchorY = 0;

        this.BindingContext = viewModel;

        // In the constructor after setting BindingContext
        viewModel.BoxNodes.CollectionChanged += OnBoxNodesChanged;

        // Pan gesture is now on the parent Grid in XAML

        // Add pointer gesture to track mouse movement for line dragging
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        CanvasSurface.GestureRecognizers.Add(pointerGesture);

        var marqueePointer = new PointerGestureRecognizer();
        marqueePointer.PointerPressed += OnMarqueePointerPressed;
        marqueePointer.PointerMoved += OnMarqueePointerMoved;
        marqueePointer.PointerReleased += OnMarqueePointerReleased;
        RootGrid.GestureRecognizers.Add(marqueePointer);

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

        WeakReferenceMessenger.Default.Register<AnchorDraggingStatus>(this, (sender, args) =>
        {
            AnchorDragging.IsAnchorDragging = args.IsAnchorDragging;
            Debug.WriteLine($"✓ Anchor dragging status changed: {args.IsAnchorDragging}");
        });

        WeakReferenceMessenger.Default.Register<ConnectionPointStatus>(this, (sender, args) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                ConnectionCanvas.Invalidate();
                return Task.CompletedTask;
            });
        });
        
        WeakReferenceMessenger.Default.Register<RebuildAnchorPointStatus>(this, (sender, args) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (args.IsAnchorAdded)
                {
                    lineConnectionManager.RebuildAnchorPointConnections(viewModel.BoxNodes);
                }
                
                return Task.CompletedTask;
            });
            
        });
        
        this.SizeChanged += (sender, eventArgs) =>
        {
            viewportWidth = this.Width;
            viewportHeight = this.Height;
            ClampPan();
        };

        UpdateTransform();
    }

    private void OnBoxNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        // {
        //     lineConnectionManager.RecalculateLines(viewModel.BoxNodes);
        //
        //     // Handle newly added items
        //     ConnectionCanvas.Invalidate(); // Redraw lines
        //     
        //     foreach (var boxNode in viewModel.BoxNodes)
        //     {
        //      //   boxNode.OnPropertyChanged(nameof(boxNode.Height));
        //     }
        // }
    }

    void ClampPan()
    {
        // Calculate based on the virtual canvas size vs the actual visible area (viewportWidth/Height)
        var scaledCanvasWidth = CanvasWidth * scale;
        var scaledCanvasHeight = CanvasHeight * scale;

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
        if (isMarqueeSelecting)
        {
            return;
        }

        // Check immediately if we're dragging - don't wait for messages
        if (lineConnectionManager.IsDragging)
        {
            Debug.WriteLine($"⚠️ Pan blocked - anchor is dragging (status: {e.StatusType})");
            return;
        }

        // If a node is being dragged or line is being drawn, don't pan the canvas
        if (DraggingStatus.IsNodeInDraggingMode || AnchorDragging.IsAnchorDragging)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                ConnectionCanvas.Invalidate();
            }

            Debug.WriteLine($"⚠️ Pan blocked - node dragging (status: {e.StatusType})");
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // Double-check here too before allowing pan to start
                if (lineConnectionManager.IsDragging || AnchorDragging.IsAnchorDragging)
                {
                    Debug.WriteLine("⚠️ Pan START blocked - anchor is dragging");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Canvas pan ACTUALLY started");
                startX = panX;
                startY = panY;
                break;

            case GestureStatus.Running:
                panX = startX + e.TotalX;
                panY = startY + e.TotalY;

                System.Diagnostics.Debug.WriteLine(
                    $"Before clamp: panX={panX}, panY={panY}, viewport={viewportWidth}x{viewportHeight}, canvas=3000x2000");
                ClampPan();
                System.Diagnostics.Debug.WriteLine($"After clamp: panX={panX}, panY={panY}");

                CanvasSurface.TranslationX = panX;
                CanvasSurface.TranslationY = panY;
                ConnectionCanvas.TranslationX = panX;
                ConnectionCanvas.TranslationY = panY;
                GridCanvas.TranslationX = panX;
                GridCanvas.TranslationY = panY;
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
    void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (this.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement nativeView)
        {
            nativeElement = nativeView;
            nativeView.IsTabStop = true;
            nativeView.PointerPressed += OnNativePointerPressed;
            nativeView.PointerWheelChanged += OnNativePointerWheelChanged;
            nativeView.KeyDown += OnNativeKeyDown;

            if (!deleteAcceleratorAttached)
            {
                var accelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
                {
                    Key = Windows.System.VirtualKey.Delete,
                    IsEnabled = true
                };
                accelerator.Invoked += OnDeleteAcceleratorInvoked;
                nativeView.KeyboardAccelerators.Add(accelerator);

                // Disable automatic tooltip
                Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(nativeView, null);

                deleteAcceleratorAttached = true;
            }
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

    void OnNativePointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        nativeElement?.Focus(Microsoft.UI.Xaml.FocusState.Pointer);
    }

    void OnDeleteAcceleratorInvoked(object sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs e)
    {
        if (DeleteSelectedNodes())
        {
            e.Handled = true;
            return;
        }

        if (lineConnectionManager.RemoveSelectedConnection())
        {
            lineConnectionManager.RebuildAnchorPointConnections(viewModel.BoxNodes);
            ConnectionCanvas.Invalidate();
            WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
            e.Handled = true;
        }
    }

    void OnNativeKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Delete)
        {
            return;
        }

        if (DeleteSelectedNodes())
        {
            e.Handled = true;
            return;
        }

        if (lineConnectionManager.RemoveSelectedConnection())
        {
            lineConnectionManager.RebuildAnchorPointConnections(viewModel.BoxNodes);
            ConnectionCanvas.Invalidate();
            WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
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

        GridCanvas.AnchorX = 0;
        GridCanvas.AnchorY = 0;
        GridCanvas.Scale = scale;
        GridCanvas.TranslationX = panX;
        GridCanvas.TranslationY = panY;
    }

#if WINDOWS
    private static bool IsMarqueeModifierPressed()
    {
        var state = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        return state.HasFlag(CoreVirtualKeyStates.Down);
    }
#endif

    private void OnMarqueePointerPressed(object? sender, MauiPointerEventArgs e)
    {
#if WINDOWS
        if (!IsMarqueeModifierPressed())
        {
            return;
        }
#endif

        if (lineConnectionManager.IsDragging || DraggingStatus.IsNodeInDraggingMode || AnchorDragging.IsAnchorDragging)
        {
            return;
        }

        var position = e.GetPosition(this);
        if (position is null)
        {
            return;
        }

        var canvasPoint = new Point((position.Value.X - panX) / scale, (position.Value.Y - panY) / scale);
        if (IsPointerOverNode(canvasPoint))
        {
            return;
        }

        isMarqueeSelecting = true;
        marqueeStartView = position.Value;
        marqueeStartCanvas = canvasPoint;
        marqueeCurrentCanvas = canvasPoint;

        SelectionRect.IsVisible = true;
        SelectionRect.TranslationX = marqueeStartView.X;
        SelectionRect.TranslationY = marqueeStartView.Y;
        SelectionRect.WidthRequest = 0;
        SelectionRect.HeightRequest = 0;
    }

    private void OnMarqueePointerMoved(object? sender, MauiPointerEventArgs e)
    {
        if (!isMarqueeSelecting)
        {
            return;
        }

        var position = e.GetPosition(this);
        if (position is null)
        {
            return;
        }

        var minX = Math.Min(marqueeStartView.X, position.Value.X);
        var minY = Math.Min(marqueeStartView.Y, position.Value.Y);
        var maxX = Math.Max(marqueeStartView.X, position.Value.X);
        var maxY = Math.Max(marqueeStartView.Y, position.Value.Y);

        SelectionRect.TranslationX = minX;
        SelectionRect.TranslationY = minY;
        SelectionRect.WidthRequest = maxX - minX;
        SelectionRect.HeightRequest = maxY - minY;

        marqueeCurrentCanvas = new Point((position.Value.X - panX) / scale, (position.Value.Y - panY) / scale);
    }

    private void OnMarqueePointerReleased(object? sender, MauiPointerEventArgs e)
    {
        if (!isMarqueeSelecting)
        {
            return;
        }

        isMarqueeSelecting = false;
        SelectionRect.IsVisible = false;

        var selection = NormalizeRect(marqueeStartCanvas, marqueeCurrentCanvas);
        if (selection.Width < MarqueeMinDistance || selection.Height < MarqueeMinDistance)
        {
            return;
        }

        foreach (var node in viewModel.BoxNodes)
        {
            var nodeRect = new Rect(
                node.XCenter,
                node.YCenter,
                node.Node.BoxDimension.Width,
                node.Node.BoxDimension.Height);

            var isInside = selection.Contains(nodeRect);
            node.IsSelected = isInside;
            node.DraggableBoxComponent?.IsSelected = isInside;
        }

        lineConnectionManager.CancelSelection();
        WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
    }

    private bool IsPointerOverNode(Point canvasPoint)
    {
        foreach (var node in viewModel.BoxNodes)
        {
            var nodeRect = new Rect(
                node.XCenter,
                node.YCenter,
                node.Node.BoxDimension.Width,
                node.Node.BoxDimension.Height);

            if (nodeRect.Contains(canvasPoint))
            {
                return true;
            }
        }

        return false;
    }

    private static Rect NormalizeRect(Point a, Point b)
    {
        var minX = Math.Min(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var maxX = Math.Max(a.X, b.X);
        var maxY = Math.Max(a.Y, b.Y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        // Allow drop
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnPointerMoved(object? sender, MauiPointerEventArgs e)
    {
        if (lineConnectionManager.IsDragging)
        {
            var position = e.GetPosition(this);
            // var position = e.GetPosition(CanvasSurface);
            if (position != null)
            {
                lineConnectionManager.UpdateDragPosition(position.Value);
                ConnectionCanvas.Invalidate();
            }
        }
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        // Get the dropped data (NodeInformationModel from ListView)
        var data = e.Data.Properties["Data"];

        if (data is INodeInformation nodeInfo)
        {
            if (nodeInfo is null)
            {
                throw new InvalidOperationException("Node information is null.");
            }
            
            // Calculate drop position accounting for canvas transformations
            var dropPosition = e.GetPosition(this);
            if (dropPosition != null)
            {
                var dropX = (dropPosition.Value.X  - panX) / scale;
                var dropY = (dropPosition.Value.Y - panY) / scale;

                dropX = SnapToGrid(dropX);
                dropY = SnapToGrid(dropY);


                nodeIo.Add(nodeInfo, new Point(dropX, dropY));

                // lineConnectionManager.RecalculateLines(viewModel.BoxNodes);

                WeakReferenceMessenger.Default.Send(new NodeActionEvent
                    { ActionEventType = NodeActionEventType.Add });

                // Add node to diagram at drop position
                // viewModel.AddNode(nodeInfo, dropX, dropY);
            }
        }
    }
    
    private void OnCanvasTapped(object? sender, MauiTappedEventArgs e)
    {
        var hasStartAnchor = lineConnectionManager.CancelSelection();

        if (hasStartAnchor)
        {
                WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
        }
        
        // This gives you the position relative to the ConnectionCanvas
        Point? position = e.GetPosition(ConnectionCanvas);
    
        if (position.HasValue)
        {
            var clickedLine = lineConnectionManager.SelectLineAtPoint(position.Value);
        
            if (clickedLine is not null)
            {
                // User clicked on a line!
                Debug.WriteLine($"Line clicked: {clickedLine.Start} -> {clickedLine.End}");
            }
            else
            {
                foreach (var node in viewModel.BoxNodes)
                {
                    node.IsSelected = false;
                    node.DraggableBoxComponent?.IsSelected = false;
                }

                lineConnectionManager.CancelSelection();
            }
            
            WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
        }

        this.Focus();
    }

    private bool DeleteSelectedNodes()
    {
        var selectedNodes = viewModel.BoxNodes.Where(n => n.IsSelected).ToList();
        if (selectedNodes.Count == 0)
        {
            return false;
        }

        var deletedAnchorIds = new HashSet<string>(
            selectedNodes.SelectMany(n => n.InputNodes.Select(i => i.Id)
                .Concat(n.OutputNodes.Select(o => o.Id))));

        var remainingNodes = viewModel.BoxNodes.Except(selectedNodes).ToList();
        foreach (var node in remainingNodes)
        {
            foreach (var input in node.Node.Inputs)
            {
                RemoveConnectionIds(input.ConnectsToParentNodeId, deletedAnchorIds);
            }

            foreach (var output in node.Node.Outputs)
            {
                RemoveConnectionIds(output.ConnectsToNodeId, deletedAnchorIds);
            }
        }

        foreach (var node in selectedNodes)
        {
            node.IsSelected = false;
            node.DraggableBoxComponent?.IsSelected = false;
            nodeIo.Nodes.Remove(node.Node);
        }

        lineConnectionManager.CancelSelection();
        lineConnectionManager.RebuildAnchorPointConnections(viewModel.BoxNodes);
        ConnectionCanvas.Invalidate();
        WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
        return true;
    }

    private static void RemoveConnectionIds(IList<string> connections, HashSet<string> deletedAnchorIds)
    {
        for (var i = connections.Count - 1; i >= 0; i--)
        {
            if (deletedAnchorIds.Contains(connections[i]))
            {
                connections.RemoveAt(i);
            }
        }
    }
    private double SnapToGrid(double value)
    {
        var size = gridSettings.Size <= 0 ? 10.0 : gridSettings.Size;
        return Math.Round(value / size) * size;
    }
}
