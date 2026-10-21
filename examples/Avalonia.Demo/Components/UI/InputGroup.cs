using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Input with something before or after the text, inside the same border — h-10 rounded-md
// border border-input. The prefix and suffix are an icon, a short text, or any control.
//   <ui:InputGroup PrefixIcon="Search" Placeholder="Search..." />
//   <ui:InputGroup PrefixText="https://" SuffixText=".com" Placeholder="example" />
//   <ui:InputGroup Placeholder="Amount">
//       <ui:InputGroup.Suffix> ...any control... </ui:InputGroup.Suffix>
//   </ui:InputGroup>
// Variant="Password" masks the text and adds an eye button after the suffix that shows it; IsPassword only masks.
public class InputGroup : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<InputGroup, bool>(nameof(IsPassword));

    public static readonly StyledProperty<InputVariant> VariantProperty =
        AvaloniaProperty.Register<InputGroup, InputVariant>(nameof(Variant));

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<InputGroup, bool>(nameof(HasError));

    public static readonly StyledProperty<IconName> PrefixIconProperty =
        AvaloniaProperty.Register<InputGroup, IconName>(nameof(PrefixIcon));

    public static readonly StyledProperty<IconName> SuffixIconProperty =
        AvaloniaProperty.Register<InputGroup, IconName>(nameof(SuffixIcon));

    public static readonly StyledProperty<string?> PrefixTextProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(PrefixText));

    public static readonly StyledProperty<string?> SuffixTextProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(SuffixText));

    // Any control before the text; replaces PrefixIcon and PrefixText.
    public static readonly StyledProperty<Control?> PrefixProperty =
        AvaloniaProperty.Register<InputGroup, Control?>(nameof(Prefix));

    // Any control after the text; replaces SuffixIcon and SuffixText.
    public static readonly StyledProperty<Control?> SuffixProperty =
        AvaloniaProperty.Register<InputGroup, Control?>(nameof(Suffix));

    private readonly TextBox _textBox;
    private readonly Border _reveal;
    private readonly Icon _revealIcon;
    private bool _revealed;
    private readonly Border _prefix = new() { VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
    private readonly Border _suffix = new() { VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
    private bool _isFocused;

    static InputGroup()
    {
        TextProperty.Changed.AddClassHandler<InputGroup>((g, e) =>
        {
            if (g._textBox.Text != (string?)e.NewValue) g._textBox.Text = (string?)e.NewValue;
        });
        PlaceholderProperty.Changed.AddClassHandler<InputGroup>((g, e) => g._textBox.PlaceholderText = (string?)e.NewValue);
        IsPasswordProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdatePassword());
        VariantProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdatePassword());
        HasErrorProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.Opacity = g.IsEnabled ? 1.0 : 0.5);
        PrefixIconProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
        SuffixIconProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
        PrefixTextProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
        SuffixTextProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
        PrefixProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
        SuffixProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateSlots());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsPassword
    {
        get => GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public InputVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public IconName PrefixIcon
    {
        get => GetValue(PrefixIconProperty);
        set => SetValue(PrefixIconProperty, value);
    }

    public IconName SuffixIcon
    {
        get => GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }

    public string? PrefixText
    {
        get => GetValue(PrefixTextProperty);
        set => SetValue(PrefixTextProperty, value);
    }

    public string? SuffixText
    {
        get => GetValue(SuffixTextProperty);
        set => SetValue(SuffixTextProperty, value);
    }

    public Control? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public Control? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    // Raised on Enter.
    public event EventHandler? Completed;

    public InputGroup()
    {
        _textBox = new TextBox
        {
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ShellPlatform.StripNativeChrome(_textBox);
        _textBox.TextChanged += (_, e) =>
        {
            Text = _textBox.Text;
            TextChanged?.Invoke(this, e);
        };
        _textBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) Completed?.Invoke(this, EventArgs.Empty);
        };
        _textBox.GotFocus += (_, _) => { _isFocused = true; UpdateVisualState(); };
        _textBox.LostFocus += (_, _) => { _isFocused = false; UpdateVisualState(); };

        Grid.SetColumn(_textBox, 1);
        Grid.SetColumn(_suffix, 2);
        (_reveal, _revealIcon) = CreateRevealButton();
        Grid.SetColumn(_reveal, 3);
        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            ColumnSpacing = 8,
            Children = { _prefix, _textBox, _suffix, _reveal }
        };
        Height = 40;
        Padding = new Thickness(12, 0);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Ibeam);
        UpdateVisualState();
    }

    // The glow color is a snapshot of a token, so it is repainted on theme changes.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ShellTheme.ThemeChanged += OnThemeChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ShellTheme.ThemeChanged -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => UpdateVisualState();

    // Clicking the padding focuses the text box too; a click on a control in the prefix or
    // suffix (a button, say) is left to that control.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.Handled && !_textBox.IsFocused) _textBox.Focus();
    }

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None)
        => _textBox.Focus(method, keyModifiers);

    private void UpdateSlots()
    {
        _prefix.Child = Prefix ?? Slot(PrefixIcon, PrefixText);
        _prefix.IsVisible = _prefix.Child != null;
        _suffix.Child = Suffix ?? Slot(SuffixIcon, SuffixText);
        _suffix.IsVisible = _suffix.Child != null;
    }

    // text-muted-foreground: a 16px icon, a text-sm label, or both.
    private static Control? Slot(IconName icon, string? text)
    {
        if (icon == IconName.None && string.IsNullOrEmpty(text)) return null;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        if (icon != IconName.None)
            row.Children.Add(new Icon { Kind = icon, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center });
        if (!string.IsNullOrEmpty(text))
        {
            var label = new TextBlock { Text = text, FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            row.Children.Add(label);
        }
        return row;
    }

    // A ghost icon button, h-7 w-7, that pulls into the padding a little.
    private (Border, Icon) CreateRevealButton()
    {
        var icon = new Icon { Kind = IconName.Eye, Size = 16, Token = ShellToken.MutedForeground };
        var button = new Border
        {
            Child = icon,
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(-4, 0, -6, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            IsVisible = false
        };
        AutomationProperties.SetName(button, "Show password");
        button.PointerEntered += (_, _) => button.Token(BackgroundProperty, ShellToken.Accent);
        button.PointerExited += (_, _) => { button.ClearToken(BackgroundProperty); button.Background = Brushes.Transparent; };
        // Handled, so the press neither focuses the button nor leaves the text box.
        button.PointerPressed += (_, e) =>
        {
            ToggleReveal();
            _textBox.Focus();
            e.Handled = true;
        };
        ShellFocus.Ring(button, button);
        button.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            ToggleReveal();
            e.Handled = true;
        };
        return (button, icon);
    }

    private void ToggleReveal()
    {
        _revealed = !_revealed;
        UpdatePassword();
    }

    private void UpdatePassword()
    {
        var password = Variant == InputVariant.Password;
        if (!password) _revealed = false;
        _textBox.PasswordChar = (IsPassword || password) && !_revealed ? '•' : default;
        _reveal.IsVisible = password;
        _revealIcon.Kind = _revealed ? IconName.EyeOff : IconName.Eye;
        AutomationProperties.SetName(_reveal, _revealed ? "Hide password" : "Show password");
    }

    private void UpdateVisualState()
    {
        var stroke = HasError ? ShellToken.Destructive : _isFocused ? ShellToken.Ring : ShellToken.Input;
        this.Token(BorderBrushProperty, stroke);
        BoxShadow = _isFocused || HasError
            ? new BoxShadows(new BoxShadow { Blur = 6, Color = ShellTheme.Get(HasError ? ShellToken.Destructive : ShellToken.Ring, 0.35) })
            : default;
    }
}
