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
}