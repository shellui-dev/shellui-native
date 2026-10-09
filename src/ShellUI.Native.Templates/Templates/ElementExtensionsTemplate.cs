using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Shared extension for overlay components - FindParentOfType for CascadingParameter-style composition
public static class ElementExtensionsTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "element-extensions",
        DisplayName = "Element Extensions",
        Description = "Shared extension methods for compositional components (FindParentOfType)",
        Category = ComponentCategory.Utility,
        FilePath = "ElementExtensions.cs",
        Dependencies = new List<string> { "shell" },
        IsAvailable = false, // Hidden from list; installed as dependency of overlay components
        Tags = new List<string> { "utility", "extensions", "composition" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Tree + animation helpers for compositional components (Dialog, Tabs, Accordion, ...).
public static class ElementExtensions
{
    // Nearest ancestor of type T. Follows portal owners, so content that was moved into the page
    // layer (DialogContent, DropdownContent, ...) still finds the component it belongs to.
    public static T? FindParentOfType<T>(this Element element) where T : Element
    {
        var p = ShellPortal.LogicalParent(element);
        while (p != null)
        {
            if (p is T t) return t;
            p = ShellPortal.LogicalParent(p);
        }
        return null;
    }

    // Every descendant of type T, depth-first. Stops descending into nested owners of the same
    // type (a Tabs inside a Tabs keeps its own triggers) when `stopAt` matches.
    public static IEnumerable<T> FindDescendantsOfType<T>(this Element root, Func<Element, bool>? stopAt = null)
    {
        foreach (var child in ((IVisualTreeElement)root).GetVisualChildren())
        {
            if (child is not Element element) continue;
            if (element is T match) yield return match;
            if (stopAt != null && stopAt(element)) continue;
            foreach (var nested in element.FindDescendantsOfType<T>(stopAt))
                yield return nested;
        }
    }

    // Height + opacity expand/collapse (shadcn's accordion-down / accordion-up).
    public static Task AnimateExpandAsync(this View view, bool open, uint length = 200)
    {
        view.AbortAnimation(""ShellExpand"");
        var tcs = new TaskCompletionSource();

        if (open)
        {
            var start = view.IsVisible ? Math.Max(view.Height, 0) : 0;
            view.HeightRequest = -1;
            view.IsVisible = true;
            var width = view.Width > 0 ? view.Width : (view.Parent as VisualElement)?.Width ?? -1;
            var target = ((IView)view).Measure(width > 0 ? width : double.PositiveInfinity, double.PositiveInfinity).Height;
            view.HeightRequest = start;
            var anim = new Animation
            {
                { 0, 1, new Animation(v => view.HeightRequest = v, start, target, Easing.CubicOut) },
                { 0, 1, new Animation(v => view.Opacity = v, view.Opacity, 1, Easing.CubicOut) }
            };
            anim.Commit(view, ""ShellExpand"", 16, length, finished: (_, cancelled) =>
            {
                if (!cancelled) view.HeightRequest = -1;
                tcs.TrySetResult();
            });
        }
        else
        {
            if (!view.IsVisible) return Task.CompletedTask;
            var start = view.Height > 0 ? view.Height : 0;
            var anim = new Animation
            {
                { 0, 1, new Animation(v => view.HeightRequest = v, start, 0, Easing.CubicIn) },
                { 0, 1, new Animation(v => view.Opacity = v, view.Opacity, 0, Easing.CubicIn) }
            };
            anim.Commit(view, ""ShellExpand"", 16, length, finished: (_, cancelled) =>
            {
                if (!cancelled)
                {
                    view.IsVisible = false;
                    view.HeightRequest = -1;
                }
                tcs.TrySetResult();
            });
        }
        return tcs.Task;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace YourProjectNamespace.Components.UI;

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

    // The latest expand/collapse per control; an older one that finishes late leaves it alone.
    private static readonly ConditionalWeakTable<Control, object> Expanding = new();

    // Grows the control from its current height to its natural height and fades it in, or the
    // reverse, then hides it. Collapsible and accordion content.
    public static async Task AnimateExpandAsync(this Control control, bool open, int milliseconds = 200)
    {
        var run = new object();
        Expanding.AddOrUpdate(control, run);
        control.ClipToBounds = true;
        control.Transitions = null;

        double start;
        double target;
        if (open)
        {
            start = control.IsVisible ? control.Bounds.Height : 0;
            control.Height = double.NaN;
            control.IsVisible = true;
            // Hidden content has no width yet: measure it at its parent's.
            var width = control.GetVisualParent() is Layoutable parent ? parent.Bounds.Width : double.PositiveInfinity;
            control.Measure(new Size(width, double.PositiveInfinity));
            target = control.DesiredSize.Height - control.Margin.Top - control.Margin.Bottom;
        }
        else
        {
            if (!control.IsVisible) return;
            start = control.Bounds.Height;
            target = 0;
        }

        control.Height = start;
        var duration = TimeSpan.FromMilliseconds(milliseconds);
        Easing easing = open ? new CubicEaseOut() : new CubicEaseIn();
        control.Transitions = new Transitions
        {
            new DoubleTransition { Property = Layoutable.HeightProperty, Duration = duration, Easing = easing },
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = duration, Easing = easing }
        };
        control.Height = target;
        control.Opacity = open ? 1 : 0;
        await Task.Delay(duration);

        if (!Expanding.TryGetValue(control, out var latest) || latest != run) return;
        control.Transitions = null;
        control.Height = double.NaN;
        if (!open) control.IsVisible = false;
    }

    // Jumps to the open or closed state without animating.
    public static void SetExpanded(this Control control, bool open)
    {
        Expanding.Remove(control);
        control.Transitions = null;
        control.Height = double.NaN;
        control.IsVisible = open;
        control.Opacity = open ? 1 : 0;
    }
}
"
    };
}
