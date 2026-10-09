using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Collapsible - single expand/collapse primitive. Compositional parent for
// CollapsibleTrigger + CollapsibleContent. Also the reusable IsOpen/animation
// primitive that AccordionItem builds on.
public static class CollapsibleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible",
        DisplayName = "Collapsible",
        Description = "Single expand/collapse container - use with CollapsibleTrigger and CollapsibleContent",
        Category = ComponentCategory.Layout,
        FilePath = "Collapsible.cs",
        Dependencies = new List<string> { "element-extensions", "collapsible-trigger", "collapsible-content" },
        Tags = new List<string> { "layout", "collapsible", "disclosure", "expand" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Expand/collapse. The trigger can wrap a Button or any view; content animates its height.
//   <ui:Collapsible Open=""{Binding IsOpen}"">
//       <ui:CollapsibleTrigger><ui:Button Text=""Toggle"" Variant=""Outline"" /></ui:CollapsibleTrigger>
//       <ui:CollapsibleContent><Label Text=""Hidden until open"" /></ui:CollapsibleContent>
//   </ui:Collapsible>
public partial class Collapsible : VerticalStackLayout
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Collapsible), false,
            BindingMode.TwoWay, propertyChanged: (b, o, n) => ((Collapsible)b).OnOpenChanged());

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    public Collapsible()
    {
        Spacing = 8;
        Loaded += (_, _) => Refresh(animate: false);
    }

    public void SetOpen(bool value) => Open = value;

    public void Toggle() => Open = !Open;

    private void OnOpenChanged()
    {
        Refresh(animate: true);
        OpenChanged?.Invoke(this, Open);
    }

    // The collapsible drives its content, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        foreach (var content in this.FindDescendantsOfType<CollapsibleContent>(e => e is Collapsible))
            content.Apply(Open, animate);
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace YourProjectNamespace.Components.UI;

// Expand/collapse. The trigger can wrap a Button or any control; content animates its height.
//   <ui:Collapsible Open=""{Binding IsOpen}"">
//       <ui:CollapsibleTrigger><ui:Button Text=""Toggle"" Variant=""Outline"" /></ui:CollapsibleTrigger>
//       <ui:CollapsibleContent><TextBlock Text=""Hidden until open"" /></ui:CollapsibleContent>
//   </ui:Collapsible>
public class Collapsible : StackPanel
{
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<Collapsible, bool>(nameof(Open), defaultBindingMode: BindingMode.TwoWay);

    static Collapsible()
    {
        OpenProperty.Changed.AddClassHandler<Collapsible>((c, _) =>
        {
            c.Refresh(animate: c.IsLoaded);
            c.OpenChanged?.Invoke(c, c.Open);
        });
    }

    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    public Collapsible()
    {
        Spacing = 8;
        Children.CollectionChanged += (_, _) => Refresh(animate: false);
    }

    public void SetOpen(bool value) => Open = value;

    public void Toggle() => Open = !Open;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh(animate: false);
    }

    // The collapsible drives its content, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        foreach (var content in this.FindDescendantsOfType<CollapsibleContent>(e => e is Collapsible))
            content.Apply(Open, animate);
    }
}
"
    };
}
