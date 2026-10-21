using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Text input — h-10 rounded-md border border-input px-3 text-sm.
// Focus: border-ring + soft ring glow. Error: border-destructive.
// The inner TextBox's Fluent chrome is stripped so this Border is the only frame.
// Variant="Password" masks the text and adds an eye button that shows it; IsPassword only masks.
public class Input : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Input, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Input, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(IsPassword));

    public static readonly StyledProperty<InputVariant> VariantProperty =
        AvaloniaProperty.Register<Input, InputVariant>(nameof(Variant));

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(HasError));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<Input, int>(nameof(MaxLength));

    private readonly TextBox _textBox;
    private readonly Border _reveal;
    private readonly Icon _revealIcon;
    private bool _revealed;
    private bool _isFocused;

    static Input()
    {
        TextProperty.Changed.AddClassHandler<Input>((i, e) =>
        {
            if (i._textBox.Text != (string?)e.NewValue) i._textBox.Text = (string?)e.NewValue;
        });
        PlaceholderProperty.Changed.AddClassHandler<Input>((i, e) => i._textBox.PlaceholderText = (string?)e.NewValue);
        IsPasswordProperty.Changed.AddClassHandler<Input>((i, _) => i.UpdatePassword());
        VariantProperty.Changed.AddClassHandler<Input>((i, _) => i.UpdatePassword());
        IsReadOnlyProperty.Changed.AddClassHandler<Input>((i, e) => i._textBox.IsReadOnly = (bool)e.NewValue!);
        MaxLengthProperty.Changed.AddClassHandler<Input>((i, e) => i._textBox.MaxLength = (int)e.NewValue!);
        HasErrorProperty.Changed.AddClassHandler<Input>((i, _) => i.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<Input>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
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

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    // 0 means no limit.
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    // Raised on Enter.
    public event EventHandler? Completed;

    public Input()
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

        (_reveal, _revealIcon) = CreateRevealButton();
        Grid.SetColumn(_reveal, 1);
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Children = { _textBox, _reveal } };
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

    // Clicking the padding focuses the text box too.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!_textBox.IsFocused) _textBox.Focus();
    }

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None)
        => _textBox.Focus(method, keyModifiers);

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
            Margin = new Thickness(4, 0, -6, 0),
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

// Password masks the text and shows an eye button that reveals it.
public enum InputVariant { Default, Password }
