using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Breadcrumb - HStack of BreadcrumbItems. The parent tells each item whether it's the
// last, and the item hides its own trailing separator accordingly (each item owns its
// separator so we never mutate the child list from OnChildAdded).
public static class BreadcrumbTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "breadcrumb",
        DisplayName = "Breadcrumb",
        Description = "Navigation trail - contains BreadcrumbItems separated by a slash",
        Category = ComponentCategory.Navigation,
        FilePath = "Breadcrumb.cs",
        Dependencies = new List<string> { "breadcrumb-item" },
        Tags = new List<string> { "navigation", "breadcrumb" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Navigation trail — items separated by chevrons; the last item hides its separator.
public partial class Breadcrumb : HorizontalStackLayout
{
    public Breadcrumb()
    {
        Spacing = 0;
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        Dispatcher.Dispatch(RefreshItemSeparators);
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        Dispatcher.Dispatch(RefreshItemSeparators);
    }

    private void RefreshItemSeparators()
    {
        var items = Children.OfType<BreadcrumbItem>().ToList();
        for (int i = 0; i < items.Count; i++)
            items[i].SetSeparatorVisible(i < items.Count - 1);
    }
}
",
        [NativePlatform.Avalonia] = @"using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// Navigation trail — items separated by chevrons; the last item hides its separator.
//   <ui:Breadcrumb>
//       <ui:BreadcrumbItem Text=""Home"" />
//       <ui:BreadcrumbItem Text=""Settings"" IsCurrent=""True"" />
//   </ui:Breadcrumb>
public class Breadcrumb : StackPanel
{
    public Breadcrumb()
    {
        Orientation = Orientation.Horizontal;
        Children.CollectionChanged += (_, _) => RefreshItemSeparators();
    }

    private void RefreshItemSeparators()
    {
        var items = Children.OfType<BreadcrumbItem>().ToList();
        for (var i = 0; i < items.Count; i++)
            items[i].SetSeparatorVisible(i < items.Count - 1);
    }
}
"
    };
}
