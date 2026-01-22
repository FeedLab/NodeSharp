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
            // Accumulate the current element's position using Bounds
            if (current is VisualElement visualElement)
            {
                absoluteX += visualElement.Bounds.X;
                absoluteY += visualElement.Bounds.Y;
            }

            // Check if we've reached the target layout
            var name = parent.AutomationId;

            if (name == topmostLayoutName)
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

    /// <summary>
    /// Finds an element by name by traversing up the visual tree through parent elements.
    /// </summary>
    /// <typeparam name="T">The type of element to find.</typeparam>
    /// <param name="element">The starting element.</param>
    /// <param name="name">The x:Name of the element to find.</param>
    /// <returns>The found element, or null if not found.</returns>
    public static T? FindInParents<T>(this Element element, string name) where T : Element
    {
        Element? parent = element.Parent;

        while (parent != null)
        {
            var found = parent.FindByName<T>(name);
            if (found != null)
            {
                return found;
            }

            parent = parent.Parent;
        }

        return null;
    }

    /// <summary>
    /// Finds an element by name by traversing down the visual tree through child elements.
    /// </summary>
    /// <typeparam name="T">The type of element to find.</typeparam>
    /// <param name="element">The starting element.</param>
    /// <param name="name">The x:Name of the element to find.</param>
    /// <returns>The found element, or null if not found.</returns>
    public static T? FindInChildren<T>(this Element element, string name) where T : Element
    {
        var found = element.FindByName<T>(name);
        if (found != null)
        {
            return found;
        }

        return null;
    }

    /// <summary>
    /// Finds an element by name by searching through sibling elements (children of the same parent).
    /// </summary>
    /// <typeparam name="T">The type of element to find.</typeparam>
    /// <param name="element">The starting element.</param>
    /// <param name="name">The x:Name of the element to find.</param>
    /// <returns>The found element, or null if not found.</returns>
    public static T? FindInSiblings<T>(this Element element, string name) where T : Element
    {
        Element? parent = element.Parent;

        if (parent == null)
        {
            return null;
        }

        var found = parent.FindByName<T>(name);
        if (found != null && !ReferenceEquals(found, element))
        {
            return found;
        }

        return null;
    }
}
