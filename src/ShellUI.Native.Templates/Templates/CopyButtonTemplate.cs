using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class CopyButtonTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "copy-button",
        DisplayName = "Copy Button",
        Description = "Copies text to the clipboard and confirms with a check",
        Category = ComponentCategory.Utility,
        FilePath = "CopyButton.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "copy", "clipboard", "button" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Copies Text to the clipboard — a ghost icon button (h-9 w-9 rounded-md, hover:bg-accent) whose
// icon turns into a check for two seconds after copying. Label adds text next to the icon.
// Usage: <ui:CopyButton Text=""shellui-native add button"" />
public partial class CopyButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(CopyButton), string.Empty,
            propertyChanged: (b, o, n) => ((CopyButton)b).Update());

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(CopyButton), string.Empty,
            propertyChanged: (b, o, n) => ((CopyButton)b).Update());

    // The text placed on the clipboard.
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event EventHandler? Copied;

    private readonly Border _border;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _copied;
    private bool _hovered;
    private int _version;

    public CopyButton()
    {
        _icon = new Icon { Name = IconName.Copy, Size = 16 };
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap,
            IsVisible = false
        };
        _border = new Border
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { _icon, _label }
            },
            HeightRequest = 36,
            MinimumWidthRequest = 36,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _border.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); _ = CopyAsync(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => _ = CopyAsync());

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        Update();
    }

    public async Task CopyAsync()
    {
        if (!IsEnabled || string.IsNullOrEmpty(Text)) return;
        try
        {
            await Clipboard.Default.SetTextAsync(Text);
        }
        catch (Exception)
        {
            // No clipboard access (e.g. a locked-down device): leave the icon as it is.
            return;
        }

        var version = ++_version;
        _copied = true;
        Update();
        Copied?.Invoke(this, EventArgs.Empty);
        await Task.Delay(2000);
        if (version != _version) return;
        _copied = false;
        Update();
    }

    private void Update()
    {
        var hasLabel = !string.IsNullOrEmpty(Label);
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = hasLabel;
        _border.Padding = new Thickness(hasLabel ? 12 : 0, 0);

        _icon.Name = _copied ? IconName.Check : IconName.Copy;
        var foreground = _copied ? ShellToken.Success : _hovered ? ShellToken.AccentForeground : ShellToken.Foreground;
        _icon.Token = foreground;
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, _hovered ? ShellToken.AccentForeground : ShellToken.Foreground);

        if (_hovered && IsEnabled)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
        Opacity = IsEnabled && !string.IsNullOrEmpty(Text) ? 1.0 : 0.5;
        SemanticProperties.SetDescription(this, _copied ? ""Copied"" : ""Copy to clipboard"");
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _border != null) Update();
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Copies Text to the clipboard — a ghost icon button (h-9 w-9 rounded-md, hover:bg-accent) whose
// icon turns into a check for two seconds after copying. Label adds text next to the icon.
// Usage: <ui:CopyButton Text=""shellui-native add button"" />
public class CopyButton : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CopyButton, string?>(nameof(Text));

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<CopyButton, string?>(nameof(Label));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private bool _copied;
    private int _version;

    static CopyButton()
    {
        TextProperty.Changed.AddClassHandler<CopyButton>((b, _) => b.Update());
        LabelProperty.Changed.AddClassHandler<CopyButton>((b, _) => b.Update());
        IsEnabledProperty.Changed.AddClassHandler<CopyButton>((b, _) => b.Update());
    }

    // The text placed on the clipboard.
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event EventHandler? Copied;

    public CopyButton()
    {
        _icon = new Icon { Kind = IconName.Copy, Size = 16, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _icon, _label }
        };
        Height = 36;
        MinWidth = 36;
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        Update();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Update();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Update();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !new Rect(Bounds.Size).Contains(e.GetPosition(this))) return;
        _ = CopyAsync();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        _ = CopyAsync();
        e.Handled = true;
    }

    public async Task CopyAsync()
    {
        if (!IsEffectivelyEnabled || string.IsNullOrEmpty(Text)) return;
        if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard) return;
        try
        {
            await clipboard.SetTextAsync(Text);
        }
        catch (Exception)
        {
            // No clipboard access: leave the icon as it is.
            return;
        }

        var version = ++_version;
        _copied = true;
        Update();
        Copied?.Invoke(this, EventArgs.Empty);
        await Task.Delay(2000);
        if (version != _version) return;
        _copied = false;
        Update();
    }

    private void Update()
    {
        var hasLabel = !string.IsNullOrEmpty(Label);
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = hasLabel;
        Padding = new Thickness(hasLabel ? 12 : 0, 0);

        var hovered = IsPointerOver && IsEffectivelyEnabled;
        _icon.Kind = _copied ? IconName.Check : IconName.Copy;
        _icon.Token = _copied ? ShellToken.Success : hovered ? ShellToken.AccentForeground : ShellToken.Foreground;
        _label.Token(TextBlock.ForegroundProperty, hovered ? ShellToken.AccentForeground : ShellToken.Foreground);

        if (hovered) this.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            // Transparent, not null, so the whole button stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = IsEnabled && !string.IsNullOrEmpty(Text) ? 1.0 : 0.5;
        AutomationProperties.SetName(this, _copied ? ""Copied"" : ""Copy to clipboard"");
    }
}
"
    };
}
