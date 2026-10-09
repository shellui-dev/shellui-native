using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// TabsTrigger - tap sets the parent Tabs' Value. Renders its own Text label with an
// active-state underline (Primary token) — no separate child needed.
public static class TabsTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "tabs-trigger",
        DisplayName = "Tabs Trigger",
        Description = "One tab button. Place inside TabsList",
        Category = ComponentCategory.Navigation,
        FilePath = "TabsTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "navigation", "tabs", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Tab button — rounded-md px-3 text-sm font-medium; active: bg-background text-foreground
// shadow-sm; inactive: text-muted-foreground.
public partial class TabsTrigger : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsTrigger), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TabsTrigger), string.Empty,
            propertyChanged: (b, o, n) => ((TabsTrigger)b)._label.Text = n as string ?? string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActive { get; private set; }

    private readonly Border _pill;
    private readonly Label _label;

    public TabsTrigger()
    {
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        _pill = new Border
        {
            Content = _label,
            Padding = new Thickness(12, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        Content = _pill;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); Select(); };
        _pill.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) Select(); });

        SetActive(false);
    }

    private void Select() => this.FindParentOfType<Tabs>()?.SetValue(Value);

    internal void SetActive(bool active)
    {
        IsActive = active;
        _label.Token(Label.TextColorProperty, active ? ShellToken.Foreground : ShellToken.MutedForeground);
        if (active)
        {
            _pill.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
            _pill.Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 1), Radius = 2, Opacity = 0.1f };
        }
        else
        {
            _pill.ClearValue(VisualElement.BackgroundColorProperty);
            _pill.BackgroundColor = Colors.Transparent;
            _pill.ClearValue(VisualElement.ShadowProperty);
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Tab button — rounded-md px-3 text-sm font-medium; active: bg-background text-foreground
// shadow-sm; inactive: text-muted-foreground. Left and Right select the neighbouring tab.
public class TabsTrigger : Border
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<TabsTrigger, string?>(nameof(Value));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TabsTrigger, string?>(nameof(Text));

    private readonly Border _pill;
    private readonly TextBlock _label;

    static TabsTrigger()
    {
        TextProperty.Changed.AddClassHandler<TabsTrigger>((t, _) => t._label.Text = t.Text ?? string.Empty);
        IsEnabledProperty.Changed.AddClassHandler<TabsTrigger>((t, _) => t.Opacity = t.IsEnabled ? 1.0 : 0.5);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActive { get; private set; }

    public TabsTrigger()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        // The ring and the active shadow are both box shadows, so each gets its own Border.
        _pill = new Border { Child = _label, Padding = new Thickness(12, 0), CornerRadius = new CornerRadius(ShellTheme.RadiusMd) };
        Child = _pill;
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        SetActive(false);
    }

    private void Select() => this.FindParentOfType<Tabs>()?.SetValue(Value);

    internal void SetActive(bool active)
    {
        IsActive = active;
        _label.Token(TextBlock.ForegroundProperty, active ? ShellToken.Foreground : ShellToken.MutedForeground);
        if (active)
        {
            _pill.Token(BackgroundProperty, ShellToken.Background);
            _pill.BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 1, Blur = 2, Color = ShellTheme.Shadow(0.1) });
        }
        else
        {
            _pill.ClearToken(BackgroundProperty);
            _pill.Background = Brushes.Transparent;
            _pill.BoxShadow = default;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        Select();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Select();
                break;
            case Key.Left or Key.Right when Parent is Panel strip:
                var tabs = strip.Children.OfType<TabsTrigger>().Where(t => t.IsEffectivelyEnabled).ToList();
                var index = tabs.IndexOf(this) + (e.Key == Key.Right ? 1 : -1);
                if (index < 0 || index >= tabs.Count) return;
                tabs[index].Focus(NavigationMethod.Directional);
                tabs[index].Select();
                break;
            default:
                return;
        }
        e.Handled = true;
    }
}
"
    };
}
