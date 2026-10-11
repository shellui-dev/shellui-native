using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ButtonGroupTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "button-group",
        DisplayName = "Button Group",
        Description = "Buttons joined into one control",
        Category = ComponentCategory.Form,
        FilePath = "ButtonGroup.cs",
        Dependencies = new List<string> { "shell", "button" },
        Tags = new List<string> { "button", "group", "segmented", "toolbar" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Button group — buttons joined into one control: only the outer corners are rounded and
// neighbouring borders overlap, so Outline buttons share a single line between them.
//   <ui:ButtonGroup>
//       <ui:Button Text=""Day"" Variant=""Outline"" />
//       <ui:Button Text=""Week"" Variant=""Outline"" />
//       <ui:Button Text=""Month"" Variant=""Outline"" />
//   </ui:ButtonGroup>
[ContentProperty(nameof(Items))]
public partial class ButtonGroup : ContentView
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(ButtonGroup), StackOrientation.Horizontal,
            propertyChanged: (b, o, n) => ((ButtonGroup)b).Arrange());

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public IList<IView> Items => _stack.Children;

    private readonly StackLayout _stack;

    public ButtonGroup()
    {
        _stack = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 0 };
        _stack.ChildAdded += (_, _) => Arrange();
        _stack.ChildRemoved += (_, _) => Arrange();
        Content = _stack;
        HorizontalOptions = LayoutOptions.Start;
        SemanticProperties.SetDescription(this, ""Button group"");
    }

    private void Arrange()
    {
        var vertical = Orientation == StackOrientation.Vertical;
        _stack.Orientation = Orientation;
        var radius = ShellTheme.RadiusMd;
        var last = _stack.Children.Count - 1;
        for (var i = 0; i <= last; i++)
        {
            if (_stack.Children[i] is not View child) continue;
            var start = i == 0 ? radius : 0;
            var end = i == last ? radius : 0;
            // -1 so two 1px borders draw as one line.
            child.Margin = i == 0 ? new Thickness(0) : vertical ? new Thickness(0, -1, 0, 0) : new Thickness(-1, 0, 0, 0);
            // Stacked buttons share the widest one's width.
            child.HorizontalOptions = vertical ? LayoutOptions.Fill : LayoutOptions.Start;
            if (child is Button button)
                button.SetCorners(vertical ? new CornerRadius(start, start, end, end) : new CornerRadius(start, end, start, end));
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Button group — buttons joined into one control: only the outer corners are rounded and
// neighbouring borders overlap, so Outline buttons share a single line between them.
//   <ui:ButtonGroup>
//       <ui:Button Text=""Day"" Variant=""Outline"" />
//       <ui:Button Text=""Week"" Variant=""Outline"" />
//       <ui:Button Text=""Month"" Variant=""Outline"" />
//   </ui:ButtonGroup>
public class ButtonGroup : Border
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ButtonGroup, Orientation>(nameof(Orientation), Orientation.Horizontal);

    private readonly StackPanel _stack = new() { Orientation = Orientation.Horizontal };

    static ButtonGroup()
    {
        OrientationProperty.Changed.AddClassHandler<ButtonGroup>((g, _) => g.Arrange());
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    [Content]
    public Controls Items => _stack.Children;

    public ButtonGroup()
    {
        Child = _stack;
        HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(this, ""Button group"");
        _stack.Children.CollectionChanged += (_, _) => Arrange();
    }

    private void Arrange()
    {
        var vertical = Orientation == Orientation.Vertical;
        _stack.Orientation = Orientation;
        var radius = ShellTheme.RadiusMd;
        var last = _stack.Children.Count - 1;
        for (var i = 0; i <= last; i++)
        {
            var child = _stack.Children[i];
            var start = i == 0 ? radius : 0;
            var end = i == last ? radius : 0;
            // -1 so two 1px borders draw as one line.
            child.Margin = i == 0 ? default : vertical ? new Thickness(0, -1, 0, 0) : new Thickness(-1, 0, 0, 0);
            // Stacked buttons share the widest one's width.
            child.HorizontalAlignment = vertical ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
            if (child is Button button)
                button.SetCorners(vertical ? new CornerRadius(start, start, end, end) : new CornerRadius(start, end, end, start));
        }
    }
}
"
    };
}
