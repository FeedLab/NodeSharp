using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Client.Component;

public partial class DraggableBoxComponent : ContentView
{
    double startX, startY;

    public static readonly BindableProperty BoxColorProperty =
        BindableProperty.Create(
            nameof(BoxColor),
            typeof(Color),
            typeof(DraggableBoxComponent),
            Colors.Gray);

    public Color BoxColor
    {
        get => (Color)GetValue(BoxColorProperty);
        set => SetValue(BoxColorProperty, value);
    }

    public DraggableBoxComponent()
    {
        InitializeComponent();

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (Parent is not AbsoluteLayout)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                var bounds = AbsoluteLayout.GetLayoutBounds(this);
                startX = bounds.X;
                startY = bounds.Y;
                break;

            case GestureStatus.Running:
                AbsoluteLayout.SetLayoutBounds(
                    this,
                    new Rect(
                        startX + e.TotalX,
                        startY + e.TotalY,
                        Width,
                        Height));
                break;
        }
    }
}
