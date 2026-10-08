using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Multi-line input — min-h-[80px] rounded-md border border-input px-3 py-2 text-sm.
// Focus: border-ring + soft ring glow. Error: border-destructive. Grows with its text.
public class Textarea : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Textarea, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Textarea, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<Textarea, bool>(nameof(HasError));

    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<Textarea, int>(nameof(MaxLength));

    private readonly TextBox _textBox;
    private bool _isFocused;

    static Textarea()
    {
        TextProperty.Changed.AddClassHandler<Textarea>((t, e) =>
        {
            if (t._textBox.Text != (string?)e.NewValue) t._textBox.Text = (string?)e.NewValue;
        });
        PlaceholderProperty.Changed.AddClassHandler<Textarea>((t, e) => t._textBox.PlaceholderText = (string?)e.NewValue);
        MaxLengthProperty.Changed.AddClassHandler<Textarea>((t, e) => t._textBox.MaxLength = (int)e.NewValue!);
        HasErrorProperty.Changed.AddClassHandler<Textarea>((t, _) => t.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<Textarea>((t, _) => t.Opacity = t.IsEnabled ? 1.0 : 0.5);
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

    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    // 0 means no limit.
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    public Textarea()
    {
        _textBox = new TextBox { FontSize = 14, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
        ShellPlatform.StripNativeChrome(_textBox);
        _textBox.MinHeight = 64; // after StripNativeChrome, which clears Fluent's minimum
        _textBox.TextChanged += (_, e) =>
        {
            Text = _textBox.Text;
            TextChanged?.Invoke(this, e);
        };
        _textBox.GotFocus += (_, _) => { _isFocused = true; UpdateVisualState(); };
        _textBox.LostFocus += (_, _) => { _isFocused = false; UpdateVisualState(); };

        Child = _textBox;
        MinHeight = 80;
        Padding = new Thickness(12, 8);
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
