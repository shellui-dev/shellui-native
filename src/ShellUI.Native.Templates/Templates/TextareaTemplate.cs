using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TextareaTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "textarea",
        DisplayName = "Textarea",
        Description = "Multi-line text input",
        Category = ComponentCategory.Form,
        FilePath = "Textarea.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "form", "input", "multiline", "textarea" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Multi-line input — min-h-[80px] rounded-md border border-input px-3 py-2 text-sm.
public partial class Textarea : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Textarea),
            string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Textarea),
            string.Empty, propertyChanged: (b, o, n) => ((Textarea)b)._editor.Placeholder = n as string ?? string.Empty);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Textarea),
            false, propertyChanged: (b, o, n) => ((Textarea)b).UpdateVisualState());

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(Textarea),
            int.MaxValue, propertyChanged: (b, o, n) => ((Textarea)b)._editor.MaxLength = (int)n);

    private readonly Editor _editor;
    private readonly Border _border;
    private bool _isFocused;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    public Textarea()
    {
        _editor = new Editor
        {
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            MinimumHeightRequest = 64,
            AutoSize = EditorAutoSizeOption.TextChanges
        };
        _editor.Token(Editor.TextColorProperty, ShellToken.Foreground);
        _editor.Token(Editor.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_editor);
        ShellFocus.Track(_editor);

        _editor.TextChanged += (s, e) => { Text = e.NewTextValue; TextChanged?.Invoke(this, e); };
        _editor.Focused += (s, e) => { _isFocused = true; UpdateVisualState(); };
        _editor.Unfocused += (s, e) => { _isFocused = false; UpdateVisualState(); };

        _border = new Border
        {
            Content = _editor,
            Padding = new Thickness(12, 8),
            MinimumHeightRequest = 80,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        Content = _border;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject b, object o, object n)
    {
        if (b is Textarea t && t._editor.Text != n as string)
            t._editor.Text = n as string ?? string.Empty;
    }

    private void UpdateVisualState()
    {
        var stroke = HasError ? ShellToken.Destructive : _isFocused ? ShellToken.Ring : ShellToken.Input;
        _border.Token(Border.StrokeProperty, stroke);
        if (_isFocused || HasError)
        {
            _border.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(ShellTheme.Get(HasError ? ShellToken.Destructive : ShellToken.Ring)),
                Offset = new Point(0, 0),
                Radius = 6,
                Opacity = 0.35f
            };
        }
        else
        {
            _border.ClearValue(VisualElement.ShadowProperty);
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

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
"
    };
}
