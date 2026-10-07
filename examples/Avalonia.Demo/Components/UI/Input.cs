using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Text input — h-10 rounded-md border border-input px-3 text-sm.
// Focus: border-ring + soft ring glow. Error: border-destructive.
// The inner TextBox's Fluent chrome is cleared so this Border is the only frame.
public class Input : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Input, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Input, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(IsPassword));

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(HasError));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<Input, int>(nameof(MaxLength));

    // Fluent's TextBox template takes its background and border brushes from these resources in
    // every state (hover, focus, disabled); overriding them on the TextBox clears that chrome.
    private static readonly string[] ClearedBrushes =
    {
        "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused", "TextControlBackgroundDisabled",
        "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled",
    };

    private readonly TextBox _textBox;
    private bool _isFocused;

    static Input()
    {
        TextProperty.Changed.AddClassHandler<Input>((i, e) =>
        {
            if (i._textBox.Text != (string?)e.NewValue) i._textBox.Text = (string?)e.NewValue;
        });
        PlaceholderProperty.Changed.AddClassHandler<Input>((i, e) => i._textBox.PlaceholderText = (string?)e.NewValue);
        IsPasswordProperty.Changed.AddClassHandler<Input>((i, e) => i._textBox.PasswordChar = (bool)e.NewValue! ? '•' : default);
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
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            MinHeight = 0,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        foreach (var key in ClearedBrushes)
            _textBox.Resources[key] = Brushes.Transparent;
        _textBox.Resources["TextControlBorderThemeThickness"] = new Thickness(0);
        _textBox.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
        _textBox.Resources["TextControlThemePadding"] = new Thickness(0);
        _textBox.Token(TextBox.ForegroundProperty, ShellToken.Foreground)
            .Token(TextBox.CaretBrushProperty, ShellToken.Foreground)
            .Token(TextBox.PlaceholderForegroundProperty, ShellToken.MutedForeground);

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

        Child = _textBox;
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

    private void UpdateVisualState()
    {
        var stroke = HasError ? ShellToken.Destructive : _isFocused ? ShellToken.Ring : ShellToken.Input;
        this.Token(BorderBrushProperty, stroke);
        BoxShadow = _isFocused || HasError
            ? new BoxShadows(new BoxShadow { Blur = 6, Color = ShellTheme.Get(HasError ? ShellToken.Destructive : ShellToken.Ring, 0.35) })
            : default;
    }
}
