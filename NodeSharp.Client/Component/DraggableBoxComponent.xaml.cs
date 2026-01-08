using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Layouts;
using NodeSharp.Client.Extension;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine.Exception;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.Component;

[SuppressMessage("ReSharper", "AsyncVoidThrowException")]
public partial class DraggableBoxComponent : ContentView
{
    double startX, startY;
    private readonly DiagramViewModel diagramViewModel;
    private readonly LineConnectionManager lineConnectionManager;
    private readonly CurvedLineDrawable curvedLineDrawable;
    private readonly IPopupService popupService;

    public static readonly BindableProperty XProperty =
        BindableProperty.Create(nameof(X), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public static readonly BindableProperty YProperty =
        BindableProperty.Create(nameof(Y), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(DraggableBoxComponent), "");

    public static readonly BindableProperty BoxColorProperty =
        BindableProperty.Create(nameof(BoxColor), typeof(Color), typeof(DraggableBoxComponent), Colors.Gray);

    public static readonly BindableProperty IsHoveredProperty =
        BindableProperty.Create(nameof(IsHovered), typeof(bool), typeof(DraggableBoxComponent), false);

    public double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
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

    public bool IsHovered
    {
        get => (bool)GetValue(IsHoveredProperty);
        set => SetValue(IsHoveredProperty, value);
    }

    public DraggableBoxComponent()
    {
        InitializeComponent();

        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        curvedLineDrawable = AppService.GetRequiredService<CurvedLineDrawable>();
        popupService = AppService.GetRequiredService<IPopupService>();


        // Update AbsoluteLayout bounds when X or Y properties change
        PropertyChanged += (sender, e) =>
        {
            var view = (VisualElement)sender!;
            if ((e.PropertyName == nameof(X) || e.PropertyName == nameof(Y)) && Parent is AbsoluteLayout)
            {
                Dispatcher.Dispatch(() =>
                {
                    AbsoluteLayout.SetLayoutBounds(this,
                        new Rect(X, Y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                });
            }
        };

        // Set initial position when parent is assigned
        this.ParentChanged += (s, e) =>
        {
            if (Parent is AbsoluteLayout)
            {
                AbsoluteLayout.SetLayoutBounds(this, new Rect(X, Y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
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
                
                boxNode.Width = view.Width;
                boxNode.Height = view.Height;
            }
            else
            {
                throw new InvalidOperationException("BindingContext is not a BoxNode.");
            }
        };

        this.BindingContextChanged += OnBindingContextChanged;

        // Add pointer hover events
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) => IsHovered = true;
        pointerGesture.PointerExited += (s, e) => IsHovered = false;
        this.GestureRecognizers.Add(pointerGesture);
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

                    X = newX;
                    Y = newY;

                    if (element.BindingContext is BoxNode boxNode)
                    {
                        boxNode.Node.X = (int)newX;
                        boxNode.Node.Y = (int)newY;
                    }

                    WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
                    lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);
                    
                    // WeakReferenceMessenger.Default.Send(new HasNodePositionChanged(true, element, this));

                    break;

                case GestureStatus.Completed:
                    WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeInDraggingMode = false });
                    break;
            }
        }
    }

    private void OnBindingContextChanged(object sender, EventArgs e)
    {
        if (BindingContext is BoxNode boxNode)
        {
            UpdateInputAnchors(boxNode);
            UpdateOutputAnchors(boxNode);
        
            boxNode.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(BoxNode.Height))
                {
                    UpdateInputAnchors(boxNode);
                    UpdateOutputAnchors(boxNode);
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
            var boxView = new BoxView
            {
                WidthRequest = 6,  // Make it larger for easier interaction
                HeightRequest = 6,
                Color = Colors.Black,
                InputTransparent = false,  // Explicitly enable input
                AnchorX = 0.5,
                AnchorY = 0.5,
                ZIndex = 1000  // Ensure it's on top
            };

            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += (s, e) =>
            {
                boxView.Color = Colors.Blue;
                boxView.Scale = 1.5;
                Debug.WriteLine($"✓ Input anchor ENTERED - IsDragging: {lineConnectionManager.IsDragging}");
            };
            pointerGesture.PointerExited += (s, e) =>
            {
                boxView.Color = Colors.Black;
                boxView.Scale = 1.0;
                Debug.WriteLine("✓ Input anchor EXITED");
            };
            pointerGesture.PointerPressed += (s, e) =>
            {
                Debug.WriteLine($"🔵 POINTER PRESSED - Setting drag state (was: {lineConnectionManager.IsDragging})");
                lineConnectionManager.StartDragging(anchor);
                WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = true });
            };
            pointerGesture.PointerReleased += (s, e) =>
            {
                Debug.WriteLine("🔵 POINTER RELEASED - Clearing drag state and redrawing");
                lineConnectionManager.EndDragging(anchor);
                
                lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);
                
                WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = false });
                WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });
            };
            boxView.GestureRecognizers.Add(pointerGesture);

            AbsoluteLayout.SetLayoutBounds(boxView, anchor.LayoutBounds);
            AbsoluteLayout.SetLayoutFlags(boxView, AbsoluteLayoutFlags.None);
            absoluteLayout.Children.Add(boxView);
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
            var boxView = new BoxView
            {
                WidthRequest = 6,
                HeightRequest = 6,
                Color = Colors.Black,
                InputTransparent = false,
                AnchorX = 0.5,
                AnchorY = 0.5,
                ZIndex = 1000  // Ensure it's on top
            };

            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += (s, e) =>
            {
                boxView.Color = Colors.Red;
                boxView.Scale = 1.5;
                Debug.WriteLine("✓ Output anchor ENTERED");
            };
            pointerGesture.PointerExited += (s, e) =>
            {
                boxView.Color = Colors.Black;
                boxView.Scale = 1.0;
                Debug.WriteLine("✓ Output anchor EXITED");
            };
            pointerGesture.PointerPressed += (s, e) =>
            {
                Debug.WriteLine($"🔴 POINTER PRESSED - Setting drag state (was: {lineConnectionManager.IsDragging})");
                lineConnectionManager.StartDragging(anchor);
                WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = true });
            };
            pointerGesture.PointerReleased += (s, e) =>
            {
                Debug.WriteLine("🔴 POINTER RELEASED - Clearing drag state and redrawing");
                lineConnectionManager.EndDragging(anchor);
                WeakReferenceMessenger.Default.Send(new AnchorDraggingStatus { IsAnchorDragging = false });
                WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = false });
            };
            boxView.GestureRecognizers.Add(pointerGesture);

            AbsoluteLayout.SetLayoutBounds(boxView, anchor.LayoutBounds);
            AbsoluteLayout.SetLayoutFlags(boxView, AbsoluteLayoutFlags.None);
            absoluteLayout.Children.Add(boxView);
        }
    }
    
    private async void OnDoubleTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Debug.WriteLine("Double-click detected!");

            if (BindingContext is not BoxNode boxNode)
            {
                throw new InvalidOperationException("OnDoubleTapped: BindingContext is not a BoxNode.");
            }

            if (boxNode.Node is not NodeFunction nodeFunction)
            {
                return;
            }
        
            await DisplayPopup(nodeFunction);
        }
        catch (Exception exception)
        {
            throw new NodeException("OnDoubleTapped: An error occurred.", exception);
        }
    }   
    
    public async Task DisplayPopup(NodeFunction nodeFunction)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(NodeFunction)] = nodeFunction
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        await popupService.ShowPopupAsync<CodeViewModel>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);
        
        var codeViewModel = AppService.GetRequiredService<CodeViewModel>();

        if (codeViewModel.HasChangedCode)
        {
            nodeFunction.FunctionData.CompileScript();
        }
        
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