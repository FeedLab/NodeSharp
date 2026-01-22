namespace NodeSharp.Nodes.Common.Extension;

public static class VisualTreeExtensions
{
    /// <summary>
    /// Returns all visual descendants (depth-first)
    /// </summary>
    public static IEnumerable<IVisualTreeElement> GetVisualDescendants(
        this IVisualTreeElement root)
    {
        if (root == null)
            yield break;

        foreach (var child in root.GetVisualChildren())
        {
            yield return child;

            foreach (var descendant in child.GetVisualDescendants())
                yield return descendant;
        }
    }

    /// <summary>
    /// Returns all visual ancestors
    /// </summary>
    public static IEnumerable<IVisualTreeElement> GetVisualAncestors(
        this IVisualTreeElement element)
    {
        var parent = element?.GetVisualParent();

        while (parent != null)
        {
            yield return parent;
            parent = parent.GetVisualParent();
        }
    }

    /// <summary>
    /// Finds the first descendant of a specific type
    /// </summary>
    public static T? FindVisualDescendant<T>(
        this IVisualTreeElement root) where T : class
    {
        return root
            .GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Finds all descendants of a specific type
    /// </summary>
    public static IEnumerable<T> FindVisualDescendants<T>(
        this IVisualTreeElement root) where T : class
    {
        return root
            .GetVisualDescendants()
            .OfType<T>();
    }

    /// <summary>
    /// Finds the first visual ancestor of a specific type
    /// </summary>
    public static T? FindVisualAncestor<T>(
        this IVisualTreeElement element) where T : class
    {
        return element
            .GetVisualAncestors()
            .OfType<T>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the position of an element relative to another element in the visual tree
    /// </summary>
    public static Point GetRelativePosition(
        this IVisualTreeElement element,
        IVisualTreeElement relativeTo)
    {
        var visualElement = (VisualElement)element;
        var relativeElement = (VisualElement)relativeTo;

        double x = 0, y = 0;
        var current = visualElement;

        while (current != null && current != relativeElement)
        {
            x += current.Bounds.X;
            y += current.Bounds.Y;
            current = current.Parent as VisualElement;
        }

        return new Point(x, y);
    }
    
    public static bool IsElementLoaded(this IVisualTreeElement element)
    {
        if (element is VisualElement ve)
            return ve.IsLoaded;

        if (element is IView view)
            return view.Handler != null; // attached to native platform view tree

        return false;
    }

    public static bool IsElementLaidOut(this IVisualTreeElement element)
    {
        // “Loaded” doesn’t guarantee layout has happened yet.
        if (element is VisualElement ve)
            return ve.IsLoaded && ve.Width > 0 && ve.Height > 0;

        if (element is IView view && view.Handler != null)
            return view.Width > 0 && view.Height > 0;

        return false;
    }
}