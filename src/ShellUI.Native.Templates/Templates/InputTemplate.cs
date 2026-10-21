using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Input component template for text entry
public static class InputTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "input",
        DisplayName = "Input",
        Description = "Text input field with placeholder, validation states, and icon support",
        Category = ComponentCategory.Form,
        FilePath = "Input.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Variants = new List<string> { "default", "error", "success" },
        Tags = new List<string> { "form", "input", "text", "field", "entry" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Text input — h-10 rounded-md border border-input px-3 text-sm.
// Focus: border-ring + soft ring glow. Error: border-destructive.
// The native Entry's own frame is stripped so this Border is the only chrome.
// Variant=""Password"" masks the text and adds an eye button that shows it; IsPassword only masks.
public partial class Input : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Input),
            string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Input),
            string.Empty, propertyChanged: (b, o, n) => ((Input)b)._entry.Placeholder = n as string ?? string.Empty);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Input),
            false, propertyChanged: (b, o, n) => ((Input)b).UpdatePassword());

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(InputVariant), typeof(Input), InputVariant.Default,
            propertyChanged: (b, o, n) => ((Input)b).UpdatePassword());

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Input),
            false, propertyChanged: (b, o, n) => ((Input)b).UpdateVisualState());

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(Input),
            false, propertyChanged: (b, o, n) => ((Input)b)._entry.IsReadOnly = (bool)n);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(Input),
            int.MaxValue, propertyChanged: (b, o, n) => ((Input)b)._entry.MaxLength = (int)n);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(Input),
            Keyboard.Default, propertyChanged: (b, o, n) => ((Input)b)._entry.Keyboard = (Keyboard)n);

    private readonly Entry _entry;
    private readonly Border _border;
    private readonly Border _reveal;
    private readonly Icon _revealIcon;
    private bool _isFocused;
    private bool _revealed;

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

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public InputVariant Variant
    {
        get => (InputVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler? Completed;

    public Input()
    {
        _entry = new Entry
        {
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            ClearButtonVisibility = ClearButtonVisibility.Never
        };
        _entry.Token(Entry.TextColorProperty, ShellToken.Foreground);
        _entry.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_entry);
        ShellFocus.Track(_entry);

        _entry.TextChanged += (s, e) =>
        {
            Text = e.NewTextValue;
            TextChanged?.Invoke(this, e);
        };
        _entry.Completed += (s, e) => Completed?.Invoke(this, e);
        _entry.Focused += (s, e) => { _isFocused = true; UpdateVisualState(); };
        _entry.Unfocused += (s, e) => { _isFocused = false; UpdateVisualState(); };

        (_reveal, _revealIcon) = CreateRevealButton();
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(_entry, 0, 0);
        row.Add(_reveal, 1, 0);

        _border = new Border
        {
            Content = row,
            Padding = new Thickness(12, 0),
            HeightRequest = 40,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };

        // Tapping the padding focuses the entry too.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _entry.Focus();
        _border.GestureRecognizers.Add(tap);

        Content = _border;
        UpdateVisualState();
    }

    // A ghost icon button, h-7 w-7, that pulls into the padding a little.
    private (Border, Icon) CreateRevealButton()
    {
        var icon = new Icon { Name = IconName.Eye, Size = 16, Token = ShellToken.MutedForeground };
        var button = new Border
        {
            Content = icon,
            WidthRequest = 28,
            HeightRequest = 28,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0, -6, 0),
            IsVisible = false
        };
        SemanticProperties.SetDescription(button, ""Show password"");
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => button.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { button.ClearValue(VisualElement.BackgroundColorProperty); button.BackgroundColor = Colors.Transparent; };
        button.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ToggleReveal();
        button.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(button, ToggleReveal);
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
        _entry.IsPassword = (IsPassword || password) && !_revealed;
        _reveal.IsVisible = password;
        _revealIcon.Name = _revealed ? IconName.EyeOff : IconName.Eye;
        SemanticProperties.SetDescription(_reveal, _revealed ? ""Hide password"" : ""Show password"");
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input && input._entry.Text != newValue as string)
            input._entry.Text = newValue as string ?? string.Empty;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void UpdateVisualState()
    {
        var stroke = HasError ? ShellToken.Destructive : _isFocused ? ShellToken.Ring : ShellToken.Input;
        _border.Token(Border.StrokeProperty, stroke);
        // The glow is always set and only turns transparent: adding or removing a shadow on Android
        // re-wraps the native view, which takes focus away from the field being typed in.
        _border.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(ShellTheme.Get(HasError ? ShellToken.Destructive : ShellToken.Ring)),
            Offset = new Point(0, 0),
            Radius = 6,
            Opacity = _isFocused || HasError ? 0.35f : 0f
        };
    }

    public new void Focus() => _entry.Focus();
    public new void Unfocus() => _entry.Unfocus();
}

// Password masks the text and shows an eye button that reveals it.
public enum InputVariant { Default, Password }
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Text input — h-10 rounded-md border border-input px-3 text-sm.
// Focus: border-ring + soft ring glow. Error: border-destructive.
// The inner TextBox's Fluent chrome is stripped so this Border is the only frame.
// Variant=""Password"" masks the text and adds an eye button that shows it; IsPassword only masks.
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
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto""), Children = { _textBox, _reveal } };
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
        AutomationProperties.SetName(button, ""Show password"");
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
        AutomationProperties.SetName(_reveal, _revealed ? ""Hide password"" : ""Show password"");
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
"
    };
}
