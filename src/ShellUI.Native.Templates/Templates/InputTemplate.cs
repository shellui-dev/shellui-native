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
        Dependencies = new List<string> { "shell" },
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
            false, propertyChanged: (b, o, n) => ((Input)b)._entry.IsPassword = (bool)n);

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

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
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

        _border = new Border
        {
            Content = _entry,
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

    public new void Focus() => _entry.Focus();
    public new void Unfocus() => _entry.Unfocus();
}
"
    };
}
