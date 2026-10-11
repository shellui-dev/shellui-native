using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Copies Text to the clipboard — a ghost icon button (h-9 w-9 rounded-md, hover:bg-accent) whose
// icon turns into a check for two seconds after copying. Label adds text next to the icon.
// Usage: <ui:CopyButton Text="shellui-native add button" />
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
        AutomationProperties.SetName(this, _copied ? "Copied" : "Copy to clipboard");
    }
}
