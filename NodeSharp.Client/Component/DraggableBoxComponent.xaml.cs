using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Layouts;
using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Exception;
using NodeSharp.Nodes.Common.Extension;
using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
#endif

namespace NodeSharp.Client.Component;

[SuppressMessage("ReSharper", "AsyncVoidThrowException")]
public partial class DraggableBoxComponent : ContentView
{
    double startX, startY;
    private readonly List<(BoxNode Node, double StartX, double StartY)> dragTargets = new();
    private readonly DiagramViewModel diagramViewModel;
    private readonly LineConnectionManager lineConnectionManager;
    private readonly CurvedLineDrawable curvedLineDrawable;
    private readonly IPopupService popupService;

    public new static readonly BindableProperty XProperty =
        BindableProperty.Create(nameof(X), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public new static readonly BindableProperty YProperty =
        BindableProperty.Create(nameof(Y), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public new static readonly BindableProperty WidthRequestProperty =
        BindableProperty.Create(nameof(WidthRequest), typeof(double), typeof(DraggableBoxComponent), 100.0);

    public new static readonly BindableProperty HeightRequestProperty =
        BindableProperty.Create(nameof(HeightRequest), typeof(double), typeof(DraggableBoxComponent), 100.0);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(DraggableBoxComponent), "");

    public static readonly BindableProperty BoxColorProperty =
        BindableProperty.Create(nameof(BoxColor), typeof(Color), typeof(DraggableBoxComponent), Colors.Gray,
            propertyChanged: OnBoxColorChanged);

    public static readonly BindableProperty BoxColorLightProperty =
        BindableProperty.Create(nameof(BoxColorLight), typeof(Color), typeof(DraggableBoxComponent), Colors.LightGray);

    public static readonly BindableProperty IsHoveredProperty =
        BindableProperty.Create(nameof(IsHovered), typeof(bool), typeof(DraggableBoxComponent), false);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(DraggableBoxComponent), false,
            propertyChanged: OnIsSelectedChanged);

    private static void OnBoxColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DraggableBoxComponent component && newValue is Color color)
        {
            // Create a lighter version of the color for gradient
            component.BoxColorLight = Color.FromRgba(
                Math.Min(color.Red + 0.15f, 1.0f),
                Math.Min(color.Green + 0.15f, 1.0f),
                Math.Min(color.Blue + 0.15f, 1.0f),
                color.Alpha
            );
        }
    }

    private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DraggableBoxComponent component && newValue is bool isSelected)
        {
            var border = component.FindByName<Border>("MainBorder");
            if (border != null)
            {
                border.Stroke = isSelected ? Colors.Blue : Color.FromArgb("#30000000");
                border.StrokeThickness = isSelected ? 3 : 2;
            }
        }
    }

    public new double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public new double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public new double WidthRequest
    {
        get => (double)GetValue(WidthRequestProperty);
        set => SetValue(WidthRequestProperty, value);
    }

    public new double HeightRequest
    {
        get => (double)GetValue(HeightRequestProperty);
        set => SetValue(HeightRequestProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color BoxColor
    {
        get => (Color)GetValue(BoxColorProperty);
        set => SetValue(BoxColorProperty, value);
    }

    public Color BoxColorLight
    {
        get => (Color)GetValue(BoxColorLightProperty);
        set => SetValue(BoxColorLightProperty, value);
    }

    public bool IsHovered
    {
        get => (bool)GetValue(IsHoveredProperty);
        set => SetValue(IsHoveredProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public DraggableBoxComponent()
    {
        InitializeComponent();

        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        curvedLineDrawable = AppService.GetRequiredService<CurvedLineDrawable>();
        popupService = AppService.GetRequiredService<IPopupService>();

        BindingContextChanged += (sender, args) =>
        {

        };
        
        Loaded += (sender, args) =>
        {
            // var leftAreaWidth = LeftAnchorArea.Width;
            // var rightAreaWidth = RightAnchorArea.Width;
            //
            // // WidthRequest = Width - leftAreaWidth - rightAreaWidth;
            //
            // if (leftAreaWidth > 0)
            // {
            //     var width = Width - leftAreaWidth - leftAreaWidth;
            //     MainBorder.WidthRequest = width;
            //     StatusComponent.WidthRequest = width;
            // }
            // else
            // {
            //     var width = Width - rightAreaWidth - rightAreaWidth;
            //     MainBorder.WidthRequest = width;
            //     StatusComponent.WidthRequest = width;
            // }
            //
            // if (Parent is VisualElement parentView)
            // {
            //     parentView.InvalidateMeasure();
            // }
        };
        
        // Update AbsoluteLayout bounds when X, Y, WidthRequest, or HeightRequest properties change
        PropertyChanged += (sender, e) =>
        {
            var view = (VisualElement)sender!;
            if ((e.PropertyName == nameof(X) || e.PropertyName == nameof(Y) ||
                 e.PropertyName == nameof(WidthRequest) || e.PropertyName == nameof(HeightRequest))
                && Parent is AbsoluteLayout)
            {
                Dispatcher.Dispatch(() =>
                {
                    AbsoluteLayout.SetLayoutBounds(this,
                        new Rect(X, Y, WidthRequest, HeightRequest));
                });
            }
        };

        // Set initial position when parent is assigned
        this.ParentChanged += (s, e) =>
        {
            if (Parent is AbsoluteLayout)
            {
                AbsoluteLayout.SetLayoutBounds(this, new Rect(X, Y, WidthRequest, HeightRequest));
            }
        };

        this.MeasureInvalidated += (sender, e) =>
        {
            var view = (VisualElement)sender!;
            Debug.WriteLine(
                $"SizeMeasureInvalidated fired by {view.GetType().Name} (Id={view.AutomationId ?? "n/a"}): Width={view.Width}, Height={view.Height}");
        };

        this.SizeChanged += (sender, args) =>
        {
            var view = (VisualElement)sender!;
            Debug.WriteLine(
                $"SizeChanged fired by {view.GetType().Name} (Id={view.AutomationId ?? "n/a"}): Width={view.Width}, Height={view.Height}");

            // if (view is null)
            // {
            //     return;
            // }

            var boxNode = (BoxNode)BindingContext;

            if (boxNode is not null && sender is DraggableBoxComponent element)
            {
                var bounds = AbsoluteLayout.GetLayoutBounds(element);

                var canvasSurface = this.Parent;
                var position = view.GetAbsolutePosition("CanvasSurface");

                boxNode.DraggableBoxComponent = element;
                boxNode.Bounds = bounds;
                boxNode.AbsolutePosition = position.Value;
                
                UpdateInputAnchors(boxNode);
                UpdateOutputAnchors(boxNode);
            }
            else
            {
                throw new InvalidOperationException("BindingContext is not a BoxNode.");
            }
        };

        this.BindingContextChanged += OnBindingContextChanged;
        this.SizeChanged += (s, e) =>
        {
            if (BindingContext is BoxNode boxNode)
            {
                boxNode.Node.RecalculateInputNodes(Height);
                boxNode.Node.RecalculateOutputNodes(Height);
            }
        };

        // Add pointer hover events
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) => IsHovered = true;
        pointerGesture.PointerExited += (s, e) => IsHovered = false;
        this.GestureRecognizers.Add(pointerGesture);

        // Add single click for selection
        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tapGesture.Tapped += OnSingleTapped;
        this.GestureRecognizers.Add(tapGesture);
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        // Don't drag the box if we're dragging from an anchor
        if (lineConnectionManager.IsDragging)
        {
            Debug.WriteLine("⚠️ Box drag blocked - anchor is being dragged");
            return;
        }

        if (Parent is not AbsoluteLayout layout)
            return;

        if (sender is VisualElement element)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    var currentBounds = AbsoluteLayout.GetLayoutBounds(this);
                    startX = X;
                    startY = Y;
                    dragTargets.Clear();

                    var selectedNodes = diagramViewModel.BoxNodes.Where(n => n.IsSelected).ToList();
                    if (selectedNodes.Count == 0 && BindingContext is BoxNode currentNode)
                    {
                        selectedNodes.Add(currentNode);
                    }

                    foreach (var node in selectedNodes)
                    {
                        dragTargets.Add((node, node.Node.X, node.Node.Y));
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"Drag started: X={X}, Y={Y}, LayoutBounds=({currentBounds.X}, {currentBounds.Y})");

                    WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeInDraggingMode = true });
                    break;

                case GestureStatus.Running:
                    double newX = startX + e.TotalX;
                    double newY = startY + e.TotalY;

                    // Get actual dimensions
                    double maxX = layout.Width - Width;
                    double maxY = layout.Height - Height;

                    // Clamp to canvas boundaries
                    newX = Math.Max(0, Math.Min(newX, maxX));
                    newY = Math.Max(0, Math.Min(newY, maxY));

                    if (dragTargets.Count > 1)
                    {
                        foreach (var target in dragTargets)
                        {
                            var targetComponent = target.Node.DraggableBoxComponent;
                            if (targetComponent is null)
                            {
                                continue;
                            }

                            var targetMaxX = layout.Width - targetComponent.Width;
                            var targetMaxY = layout.Height - targetComponent.Height;
                            var targetX = target.StartX + e.TotalX;
                            var targetY = target.StartY + e.TotalY;

                            targetX = Math.Max(0, Math.Min(targetX, targetMaxX));
                            targetY = Math.Max(0, Math.Min(targetY, targetMaxY));

                            targetComponent.X = targetX;
                            targetComponent.Y = targetY;
                            target.Node.Node.X = (int)targetX;
                            target.Node.Node.Y = (int)targetY;
                        }
                    }
                    else
                    {
                        X = newX;
                        Y = newY;

                        if (element.BindingContext is BoxNode boxNode)
                        {
                            boxNode.Node.X = (int)newX;
                            boxNode.Node.Y = (int)newY;
                        }
                    }

                    WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });

                    // WeakReferenceMessenger.Default.Send(new HasNodePositionChanged(true, element, this));

                    break;

                case GestureStatus.Completed:
                    lineConnectionManager.RebuildAnchorPointConnections(diagramViewModel.BoxNodes);
                    dragTargets.Clear();

                    WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeInDraggingMode = false });
                    WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
                    break;
            }
        }
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is BoxNode boxNode)
        {
            // boxNode.Node.RecalculateInputNodes(Height);
            // boxNode.Node.RecalculateOutputNodes(Height);
            //
            // UpdateInputAnchors(boxNode);
            // UpdateOutputAnchors(boxNode);
            //
            // boxNode.PropertyChanged += (s, args) =>
            // {
            //     if (args.PropertyName == nameof(BoxNode.Node.BoxDimension.Height))
            //     {
            //         boxNode.Node.RecalculateInputNodes(Height);
            //         boxNode.Node.RecalculateOutputNodes(Height);
            //
            //         UpdateInputAnchors(boxNode);
            //         UpdateOutputAnchors(boxNode);
            //     }
            // };

            BoxNodeBodyContainer.Content = boxNode.Node.NodeBodyComponent;
            BoxNodeStatusContainer.Content = boxNode.Node.BoxNodeStatusComponent;

            // Sync selection state
            boxNode.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(BoxNode.IsSelected))
                {
                    IsSelected = boxNode.IsSelected;
                }
            };
        }
    }

    private void UpdateInputAnchors(BoxNode boxNode)
    {
        var leftAnchorArea = this.FindByName<Grid>("LeftAnchorArea");
        if (leftAnchorArea == null) return;

        // Ensure the grid can receive input
        leftAnchorArea.InputTransparent = false;

        var component = leftAnchorArea.Children.OfType<BoxAnchorLeftComponent>().FirstOrDefault();
        if (component == null) return;

        var absoluteLayout = component.FindByName<AbsoluteLayout>("CanvasLeftAnchorArea");
        if (absoluteLayout == null) return;

        absoluteLayout.InputTransparent = false;
        absoluteLayout.Children.Clear();

        foreach (var anchor in boxNode.InputNodes)
        {
            // var boxViewLine = new BoxView
            // {
            //     WidthRequest = 50, // Make it larger for easier interaction
            //     HeightRequest = 1,
            //     Color = Colors.DarkGray,
            //     InputTransparent = false, // Explicitly enable input
            //     AnchorX = 0.5,
            //     AnchorY = 0.5,
            //     ZIndex = 500
            // };
            //
            // var boxView = new Ellipse
            // {
            //     WidthRequest = 10,
            //     HeightRequest = 10,
            //     Fill = Colors.Transparent,
            //     Stroke = Colors.Black,
            //     StrokeThickness = 2,
            //     InputTransparent = false,
            //     ZIndex = 1000
            // };

            var anchorNode= new AnchorComponent(anchor);
            anchor.AnchorComponent = anchorNode;
            
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += (s, e) =>
            {
                Debug.WriteLine($"✓ Input anchor ENTERED - IsDragging: {lineConnectionManager.IsDragging}");
            };
            pointerGesture.PointerExited += (s, e) =>
            {
                Debug.WriteLine("✓ Input anchor EXITED");
            };
            pointerGesture.PointerPressed += (s, e) =>
            {
                Debug.WriteLine($"🔵 POINTER PRESSED - Setting drag state (was: {lineConnectionManager.IsDragging})");
                
               
//                WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = true });
            };
            pointerGesture.PointerReleased += (s, e) =>
            {
                Debug.WriteLine("🔵 POINTER RELEASED - Clearing drag state and redrawing");

            };
            anchorNode.GestureRecognizers.Add(pointerGesture);

            AbsoluteLayout.SetLayoutFlags(anchorNode, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(anchorNode, new Rect(0, anchor.Y - (25.0 / 2.0), 60, 25));
            absoluteLayout.Children.Add(anchorNode);
        }
    }

    private void UpdateOutputAnchors(BoxNode boxNode)
    {
        var rightAnchorArea = this.FindByName<Grid>("RightAnchorArea");
        if (rightAnchorArea == null) return;

        rightAnchorArea.InputTransparent = false;

        var component = rightAnchorArea.Children.OfType<BoxAnchorRightComponent>().FirstOrDefault();
        if (component == null) return;

        var absoluteLayout = component.FindByName<AbsoluteLayout>("CanvasRightAnchorArea");
        if (absoluteLayout == null) return;

        absoluteLayout.InputTransparent = false;
        absoluteLayout.Children.Clear();

        foreach (var anchor in boxNode.OutputNodes)
        {
            var anchorNode= new AnchorComponent(anchor);
            anchor.AnchorComponent = anchorNode;
            
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += (s, e) =>
            {
                Debug.WriteLine("✓ Output anchor ENTERED");
            };
            pointerGesture.PointerExited += (s, e) =>
            {
                // boxView.Stroke = Colors.Black;
                // boxView.Scale = 1.0;
                Debug.WriteLine("✓ Output anchor EXITED");
            };
            pointerGesture.PointerPressed += (s, e) =>
            {
                // var canvasSurface = this.FindByName<Element>("ConnectionCanvas");
                // var canvasSurface = this.FindInParents<GraphicsView>("ConnectionCanvas");
                
                // var position = e.GetPosition(this);
                //
                // if (position.HasValue)
                // {
                //     lineConnectionManager.StartDragging(anchor, position.Value);
                //
                //     // anchor.X = position.Value.X;
                //     // anchor.Y = position.Value.Y;
                // }
                //
                // WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = true });
            };
            pointerGesture.PointerReleased += (s, e) =>
            {
                // Debug.WriteLine("🔴 POINTER RELEASED - Clearing drag state and redrawing");
                //
                // var position = e.GetPosition(this);
                // if (position.HasValue)
                // {
                //     anchor.X = position.Value.X;
                //     anchor.Y = position.Value.Y;
                // }
                //
                // lineConnectionManager.EndDragging(anchor);
                // WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = false });
                // WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
            };
            anchorNode.GestureRecognizers.Add(pointerGesture);

            // AbsoluteLayout.SetLayoutBounds(boxView, anchor.LayoutBounds);
            // AbsoluteLayout.SetLayoutBounds(anchorNode, new Rect(anchor.X - 5, anchor.Y - 5, 10, 10));
            AbsoluteLayout.SetLayoutFlags(anchorNode, AbsoluteLayoutFlags.None);

            AbsoluteLayout.SetLayoutBounds(anchorNode, new Rect(0, anchor.Y- (25.0 / 2.0), 60, 25));
            // AbsoluteLayout.SetLayoutFlags(boxViewLine, AbsoluteLayoutFlags.None);
            //
            // absoluteLayout.Children.Add(boxViewLine);
            absoluteLayout.Children.Add(anchorNode);
        }
    }

    private void OnSingleTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        try
        {
            if (BindingContext is not BoxNode boxNode)
            {
                return;
            }

            // Check if Ctrl key is pressed (Windows-only)
            bool isCtrlPressed = false;
#if WINDOWS
            var state = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            isCtrlPressed = state.HasFlag(CoreVirtualKeyStates.Down);
#endif

            if (isCtrlPressed)
            {
                // Toggle selection for this box
                boxNode.IsSelected = !boxNode.IsSelected;
                IsSelected = boxNode.IsSelected;
            }
            else
            {
                // Deselect all other boxes, select this one
                foreach (var node in diagramViewModel.BoxNodes)
                {
                    node.IsSelected = false;
                    node.DraggableBoxComponent?.IsSelected = false;
                }

                boxNode.IsSelected = true;
                IsSelected = true;
            }

            Debug.WriteLine($"Box {boxNode.Node.Name} selected: {boxNode.IsSelected}");
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"OnSingleTapped error: {exception.Message}");
        }
    }

    private async void OnDoubleTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        try
        {
            Debug.WriteLine("Double-click detected!");

            if (BindingContext is not BoxNode boxNode)
            {
                throw new InvalidOperationException("OnDoubleTapped: BindingContext is not a BoxNode.");
            }

            // if (boxNode.Node is NodeFunction nodeFunction)
            // {
            //     await DisplayPopup(nodeFunction);
            //     return;
            // }

            await boxNode.Node.DisplayNodeConfigurationPopup();
        }
        catch (Exception exception)
        {
            throw new NodeException("OnDoubleTapped: An error occurred.", exception);
        }
    }

    // public async Task DisplayPopup(NodeFunction nodeFunction)
    // {
    //     var queryAttributes = new Dictionary<string, object>
    //     {
    //         [nameof(NodeFunction)] = nodeFunction
    //     };
    //
    //     var popupOptions = new PopupOptions
    //     {
    //         CanBeDismissedByTappingOutsideOfPopup = false
    //     };
    //
    //     await popupService.ShowPopupAsync<CodeViewModel>(
    //         Shell.Current,
    //         options: popupOptions,
    //         shellParameters: queryAttributes);
    //
    //     var codeViewModel = AppService.GetRequiredService<CodeViewModel>();
    //
    //     if (codeViewModel.HasChangedCode)
    //     {
    //         nodeFunction.FunctionData.CompileScript();
    //     }
    //
    // }

    private async void OnPointerEntered(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BindingContext is BoxNode boxNode)
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(BaseNode)] = boxNode.Node
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                PageOverlayColor = Colors.Transparent
            };

            await popupService.ShowPopupAsync<LastOutputMessageTooltipViewModel>(
                Shell.Current,
                options: popupOptions,
                queryAttributes);
        }
    }

    private async void OnPointerExited(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        //   await popupService.ClosePopupAsync(Shell.Current, true);
    }
}

public class NodeDraggingStatus
{
    public bool IsNodeInDraggingMode { get; set; }
}

public class HasNodePositionChanged(bool isDirty, VisualElement element, DraggableBoxComponent draggableBox)
{
    public bool IsDirty { get; } = isDirty;
    public VisualElement? Element { get; } = element;
    public DraggableBoxComponent DraggableBox { get; } = draggableBox;
}

public class AnchorDraggingStatus
{
    public bool IsAnchorDragging { get; set; }
}
