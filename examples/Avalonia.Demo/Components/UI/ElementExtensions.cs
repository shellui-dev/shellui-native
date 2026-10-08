using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.LogicalTree;

namespace AvaloniaDemo.Components.UI;

// Logical-tree helpers for compositional components (Dialog, Dropdown, ...).
public static class ElementExtensions
{
    // Nearest logical ancestor of type T. Floating content's logical parent is its Popup, so a
    // part shown in the overlay layer (DialogClose, DropdownItem) still finds its component.
    public static T? FindParentOfType<T>(this StyledElement element) where T : class
    {
        for (var p = element.Parent; p != null; p = p.Parent)
            if (p is T match) return match;
        return null;
    }

    // Every logical descendant of type T, depth-first. Stops descending into an element that
    // matches `stopAt` (a Tabs inside a Tabs keeps its own triggers).
    public static IEnumerable<T> FindDescendantsOfType<T>(this ILogical root, Func<ILogical, bool>? stopAt = null)
    {
        foreach (var child in root.LogicalChildren)
        {
            if (child is T match) yield return match;
            if (stopAt != null && stopAt(child)) continue;
            foreach (var nested in child.FindDescendantsOfType<T>(stopAt))
                yield return nested;
        }
    }
}
