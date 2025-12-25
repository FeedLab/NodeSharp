using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace NodeSharp.Client.Component;

public partial class DraggableBoxComponent : ContentView
{
    double startX, startY;

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

        // Update AbsoluteLayout bounds when X or Y properties change
        PropertyChanged += (s, e) =>
        {
            if ((e.PropertyName == nameof(X) || e.PropertyName == nameof(Y)) && Parent is AbsoluteLayout)
            {
                Dispatcher.Dispatch(() =>
                {
                    AbsoluteLayout.SetLayoutBounds(this, new Rect(X, Y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
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
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (Parent is not AbsoluteLayout layout)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                var currentBounds = AbsoluteLayout.GetLayoutBounds(this);
                startX = X;
                startY = Y;

                System.Diagnostics.Debug.WriteLine($"Drag started: X={X}, Y={Y}, LayoutBounds=({currentBounds.X}, {currentBounds.Y})");

                WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeDragging = true });
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
                break;

            case GestureStatus.Completed:
                WeakReferenceMessenger.Default.Send(new NodeDraggingStatus { IsNodeDragging = false });
                break;
        }
    }
}
