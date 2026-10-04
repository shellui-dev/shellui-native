using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class BreadcrumbItemTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "breadcrumb-item",
        DisplayName = "Breadcrumb Item",
        Description = "One crumb in a Breadcrumb - text + tap event. Renders its own trailing separator (hidden on the last item)",
        Category = ComponentCategory.Navigation,
        FilePath = "BreadcrumbItem.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "navigation", "breadcrumb", "item" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Breadcrumb link — text-sm text-muted-foreground hover:text-foreground; the current page is
// text-foreground and not clickable. A chevron separator follows every item but the last.
public partial class BreadcrumbItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(BreadcrumbItem), string.Empty,
            propertyChanged: (b, o, n) => ((BreadcrumbItem)b).UpdateVisualState());

    public static readonly BindableProperty IsCurrentProperty =
        BindableProperty.Create(nameof(IsCurrent), typeof(bool), typeof(BreadcrumbItem), false,
            propertyChanged: (b, o, n) => ((BreadcrumbItem)b).UpdateVisualState());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsCurrent
    {
        get => (bool)GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Label _text;
    private readonly Icon _separator;
    private bool _hovered;

    public BreadcrumbItem()
    {
        _text = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _separator = new Icon
        {
            Name = IconName.ChevronRight,
            Size = 14,
            Token = ShellToken.MutedForeground,
            Margin = new Thickness(6, 0)
        };
        Content = new HorizontalStackLayout { Spacing = 0, Children = { _text, _separator } };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; UpdateVisualState(); };
        pointer.PointerExited += (_, _) => { _hovered = false; UpdateVisualState(); };
        _text.GestureRecognizers.Add(pointer);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsCurrent) return; ShellFocus.FocusPressed(this); Clicked?.Invoke(this, EventArgs.Empty); };
        _text.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (!IsCurrent) Clicked?.Invoke(this, EventArgs.Empty); }, when: () => !IsCurrent);

        UpdateVisualState();
    }

    public void SetSeparatorVisible(bool visible) => _separator.IsVisible = visible;

    private void UpdateVisualState()
    {
        _text.Text = Text ?? string.Empty;
        _text.Token(Label.TextColorProperty,
            IsCurrent || _hovered ? ShellToken.Foreground : ShellToken.MutedForeground);
    }
}
"
    };
}
