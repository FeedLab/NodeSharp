using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class MultiBoxComponent : ContentView
{
    public MultiBoxViewModel ViewModel { get; }
    private readonly Border[] boxes;

    public MultiBoxComponent(int boxCount, Color onColor, Color offColor)
    {
        ViewModel = new MultiBoxViewModel(boxCount, onColor, offColor);
        this.BindingContext = ViewModel;

        InitializeComponent();

        boxes = [Box0, Box1, Box2, Box3, Box4, Box5, Box6, Box7];
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0) return;

        var spacing = 4;
        var totalSpacing = spacing * (ViewModel.BoxCount - 1);
        var availableWidth = Width - totalSpacing - 8; // Account for margin
        var boxWidth = availableWidth / ViewModel.BoxCount;

        for (int i = 0; i < ViewModel.BoxCount; i++)
        {
            var box = boxes[i];
            var x = i * (boxWidth + spacing);

            AbsoluteLayout.SetLayoutBounds(box, new Rect(x, 0, boxWidth, 12));
            AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.None);
        }
    }
}
