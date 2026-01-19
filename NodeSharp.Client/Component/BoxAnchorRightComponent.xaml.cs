using System.Diagnostics;
using Microsoft.Maui.Controls.Shapes;
using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Extension;

namespace NodeSharp.Client.Component;

public partial class BoxAnchorRightComponent : ContentView
{
    public BoxAnchorRightComponent()
    {
        InitializeComponent();
        
        CanvasRightAnchorArea.BindingContextChanged += OnBindingContextChanged;
        
        CanvasRightAnchorArea.SizeChanged += CanvasRightAnchorAreaOnSizeChanged;
        
        CanvasRightAnchorArea.ChildAdded += CanvasRightAnchorAreaOnChildAdded;
        
        CanvasRightAnchorArea.MeasureInvalidated += CanvasRightAnchorAreaOnMeasureInvalidated;
        
        this.Loaded += OnLoaded;
        
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var diagramView = this.FindInParents<DiagramViewComponent>("DrawingView");
        var canvasSurface = this.FindInParents<GraphicsView>("ConnectionCanvas");
    
        if (canvasSurface != null)
        {
            var children = CanvasRightAnchorArea.GetVisualTreeDescendants().OfType<Ellipse>();

            foreach (var child in children)
            {
                var childX = child.X;
                var childY = child.Y;
            
                // Walk up the visual tree accumulating offsets
                var current = child.Parent as VisualElement;
                while (current != null && current != canvasSurface)
                {
                    childX += current.X;
                    childY += current.Y;
                
                    // Also check for TranslationX/Y if elements are transformed
                    childX += current.TranslationX;
                    childY += current.TranslationY;
                
                    current = current.Parent as VisualElement;
                }
            
                // childX and childY are now relative to ConnectionCanvas
                var xPosition = childX;
                var yPosition = childY;
            }
        }
    }

    private void CanvasRightAnchorAreaOnMeasureInvalidated(object? sender, EventArgs e)
    {
    }

    private void CanvasRightAnchorAreaOnChildAdded(object? sender, ElementEventArgs e)
    {
        var canvasSurface = this.FindInParents<GraphicsView>("ConnectionCanvas");
        
    }

    private void CanvasRightAnchorAreaOnSizeChanged(object? sender, EventArgs e)
    {
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is BoxNode boxNode)
        {
            // UpdateInputAnchors(boxNode);
            // UpdateOutputAnchors(boxNode);

            boxNode.PropertyChanged += (s, args) =>
            {
                Debug.WriteLine("BoxNode changed:");
                // if (args.PropertyName == nameof(BoxNode.Height))
                // {
                //     UpdateInputAnchors(boxNode);
                //     UpdateOutputAnchors(boxNode);
                // }
            };
        }
    }
}