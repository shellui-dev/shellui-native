namespace MAUI.Demo.Components.UI;

/// <summary>Extensions for compositional components (Dialog, Drawer, etc.) - find parent in visual tree.</summary>
public static class ElementExtensions
{
    public static T? FindParentOfType<T>(this Element element) where T : Element
    {
        var p = element.Parent;
        while (p != null)
        {
            if (p is T t) return t;
            p = p.Parent;
        }
        return null;
    }
}
