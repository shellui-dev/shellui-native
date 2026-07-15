using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Input component styled like shadcn/ui
// h-9 rounded-md border bg-transparent px-3 py-1 text-sm shadow-xs
// Focus: ring-[3px] ring-ring/50 border-ring
// Error: border-destructive ring-destructive/20
public partial class Input : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Input), 
            string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Input), 
            string.Empty, propertyChanged: OnPlaceholderChanged);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Input), 
            false, propertyChanged: OnIsPasswordChanged);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Input), 
            false, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(Input), 
            false, propertyChanged: OnIsReadOnlyChanged);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(Input), 
            int.MaxValue, propertyChanged: OnMaxLengthChanged);

    private readonly Entry _entry;
    private readonly Border _border;
    private readonly Border _focusRing;
    private readonly Grid _container;
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

    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler? Completed;

    public Input()
    {
        _entry = new Entry
        {
            BackgroundColor = Colors.Transparent,
            FontSize = 14, // text-sm
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        // Remove native Entry border/styling on Windows
        _entry.HandlerChanged += OnEntryHandlerChanged;
        
        _entry.TextChanged += (s, e) => 
        {
            Text = e.NewTextValue;
            TextChanged?.Invoke(this, e);
        };
        _entry.Completed += (s, e) => Completed?.Invoke(this, e);
        _entry.Focused += (s, e) => { _isFocused = true; UpdateVisualState(); };
        _entry.Unfocused += (s, e) => { _isFocused = false; UpdateVisualState(); };

        // Focus ring (outer glow effect) - ring-[3px] ring-ring/50
        _focusRing = new Border
        {
            StrokeThickness = 3,
            Stroke = Colors.Transparent,
            BackgroundColor = Colors.Transparent,
            Padding = 0,
            IsVisible = false
        };

        // Main input border - h-9 rounded-md border
        _border = new Border
        {
            Content = _entry,
            Padding = new Thickness(12, 0), // px-3, vertical handled by height
            HeightRequest = 36, // h-9 = 2.25rem = 36px
            StrokeThickness = 1
        };

        // Stack focus ring behind border
        _container = new Grid();
        _container.Children.Add(_focusRing);
        _container.Children.Add(_border);

        Content = _container;
        
        // Listen for theme changes
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) => UpdateVisualState();
        }
        
        UpdateVisualState();
    }

    private void OnEntryHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (_entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            // Remove native TextBox border completely
            textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            textBox.BorderBrush = null;
            textBox.Background = null;
            
            // Remove focus visuals
            textBox.FocusVisualPrimaryThickness = new Microsoft.UI.Xaml.Thickness(0);
            textBox.FocusVisualSecondaryThickness = new Microsoft.UI.Xaml.Thickness(0);
            
            // Remove padding so our custom padding works
            textBox.Padding = new Microsoft.UI.Xaml.Thickness(0);
        }
#endif
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input && input._entry.Text != newValue as string)
            input._entry.Text = newValue as string ?? string.Empty;
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.Placeholder = newValue as string ?? string.Empty;
    }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.IsPassword = (bool)newValue;
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.IsReadOnly = (bool)newValue;
    }

    private static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.MaxLength = (int)newValue;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var isDark = ShellTheme.IsDarkMode;
        
        // Border colors
        Color borderColor;
        Color focusRingColor;
        
        if (HasError)
        {
            // Error state: border-destructive ring-destructive/20
            borderColor = ShellTheme.Destructive;
            focusRingColor = ShellTheme.Destructive.WithAlpha(0.2f);
        }
        else if (_isFocused)
        {
            // Focus state: border-ring ring-ring/50
            borderColor = ShellTheme.Primary;
            focusRingColor = ShellTheme.Primary.WithAlpha(0.3f);
        }
        else
        {
            // Default: border-input
            borderColor = ShellTheme.BorderInput;
            focusRingColor = Colors.Transparent;
        }

        // Main border styling
        _border.BackgroundColor = isDark 
            ? Color.FromArgb("#1E293B").WithAlpha(0.3f) // dark:bg-input/30
            : Colors.Transparent; // bg-transparent
        _border.Stroke = borderColor;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 6 }; // rounded-md
        
        // Subtle shadow - shadow-xs
        _border.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#10000000")),
            Offset = new Point(0, 1),
            Radius = 2,
            Opacity = isDark ? 0.3f : 0.1f
        };

        // Focus ring styling
        _focusRing.IsVisible = _isFocused || HasError;
        _focusRing.Stroke = focusRingColor;
        _focusRing.StrokeShape = new RoundRectangle { CornerRadius = 8 }; // Slightly larger radius
        _focusRing.Margin = new Thickness(-3); // Expand outward for ring effect

        // Entry text styling
        _entry.PlaceholderColor = ShellTheme.ForegroundMuted; // placeholder:text-muted-foreground
        _entry.TextColor = ShellTheme.Foreground;

        // Disabled state
        Opacity = IsEnabled ? 1.0 : 0.5;
    }

    public new void Focus() => _entry.Focus();
    public new void Unfocus() => _entry.Unfocus();
}
