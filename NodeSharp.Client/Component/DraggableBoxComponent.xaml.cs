using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class DraggableBoxComponent : ContentView
{
    double startX, startY;
    private readonly DiagramViewModel diagramViewModel;

    public static readonly BindableProperty XProperty =
        BindableProperty.Create(nameof(X), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public static readonly BindableProperty YProperty =
        BindableProperty.Create(nameof(Y), typeof(double), typeof(DraggableBoxComponent), 0.0);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(DraggableBoxComponent), "");

    public static readonly BindableProperty BoxColorProperty =
        BindableProperty.Create(nameof(BoxColor), typeof(Color), typeof(DraggableBoxComponent), Colors.Gray);

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

    public DraggableBoxComponent()
    {
        InitializeComponent();

        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();

        
        // Update AbsoluteLayout bounds when X or Y properties change
        PropertyChanged += (s, e) =>
        {
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

            if (boxNode is not null)
            {
                boxNode.Width = view.Width;
                boxNode.Height = view.Height;
            }
            else
            {
                throw new InvalidOperationException("BindingContext is not a BoxNode.");
            }
        };
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
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

                    WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { Dummy = false });

                    // WeakReferenceMessenger.Default.Send(new HasNodePositionChanged(true, element, this));

                    break;

                case GestureStatus.Completed:
                    WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeInDraggingMode = false });
                    break;
            }
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