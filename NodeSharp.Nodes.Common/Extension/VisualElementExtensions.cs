namespace NodeSharp.Nodes.Common.Extension;

public static class VisualElementExtensions
{
    /// <summary>
    /// Gets the absolute position of a VisualElement relative to a topmost AbsoluteLayout by name.
    /// </summary>
    /// <param name="element">The element to get the position for.</param>
    /// <param name="topmostLayoutName">The AutomationId or class name of the topmost AbsoluteLayout.</param>
    /// <returns>A Point containing the absolute X and Y coordinates, or null if the topmost layout is not found.</returns>
    public static Point? GetAbsolutePosition(this VisualElement element, string topmostLayoutName)
    {
        double absoluteX = 0;
        double absoluteY = 0;

        var current = element;
        Element? parent = element.Parent;

        while (parent != null)
        {
            // Get the layout bounds for the current element
            if (current is VisualElement visualElement)
            {
                var bounds = AbsoluteLayout.GetLayoutBounds(visualElement);
                absoluteX += bounds.X;
                absoluteY += bounds.Y;
            }

            // Check if we've reached the target layout
            var name = parent.AutomationId;
            
            if (parent is AbsoluteLayout && name == topmostLayoutName)
            {
                // Found the target layout, return the accumulated position
                return new Point(absoluteX, absoluteY);
            }

            // Move up the tree
            current = parent as VisualElement;
            parent = parent.Parent;
        }

        // Topmost layout not found
        return null;
    }
}
