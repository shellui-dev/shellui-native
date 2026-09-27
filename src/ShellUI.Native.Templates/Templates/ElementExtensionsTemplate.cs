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
        Dependencies = new List<string>(),
        IsAvailable = false, // Hidden from list; installed as dependency of overlay components
        Tags = new List<string> { "utility", "extensions", "composition" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Tree + animation helpers for compositional components (Dialog, Tabs, Accordion, ...).
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
"
    };
}
