using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Input with something before or after the text, inside the same border — h-10 rounded-md
// border border-input. The prefix and suffix are an icon, a short text, or any view.
//   <ui:InputGroup PrefixIcon="Search" Placeholder="Search..." />
//   <ui:InputGroup PrefixText="https://" SuffixText=".com" Placeholder="example" />
//   <ui:InputGroup Placeholder="Amount">
//       <ui:InputGroup.Suffix> ...any view... </ui:InputGroup.Suffix>
//   </ui:InputGroup>
public partial class InputGroup : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(InputGroup), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) =>
            {
                var group = (InputGroup)b;
                if (group._entry.Text != n as string) group._entry.Text = n as string ?? string.Empty;
            });

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(InputGroup), string.Empty,
            propertyChanged: (b, o, n) => ((InputGroup)b)._entry.Placeholder = n as string ?? string.Empty);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(InputGroup), false,
            propertyChanged: (b, o, n) => ((InputGroup)b)._entry.IsPassword = (bool)n);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(InputGroup), false,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateVisualState());

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputGroup), Keyboard.Default,
            propertyChanged: (b, o, n) => ((InputGroup)b)._entry.Keyboard = (Keyboard)n);

    public static readonly BindableProperty PrefixIconProperty =
        BindableProperty.Create(nameof(PrefixIcon), typeof(IconName), typeof(InputGroup), IconName.None,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

    public static readonly BindableProperty SuffixIconProperty =
        BindableProperty.Create(nameof(SuffixIcon), typeof(IconName), typeof(InputGroup), IconName.None,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

    public static readonly BindableProperty PrefixTextProperty =
        BindableProperty.Create(nameof(PrefixText), typeof(string), typeof(InputGroup), string.Empty,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

    public static readonly BindableProperty SuffixTextProperty =
        BindableProperty.Create(nameof(SuffixText), typeof(string), typeof(InputGroup), string.Empty,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

    // Any view before the text; replaces PrefixIcon and PrefixText.
    public static readonly BindableProperty PrefixProperty =
        BindableProperty.Create(nameof(Prefix), typeof(View), typeof(InputGroup), null,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

    // Any view after the text; replaces SuffixIcon and SuffixText.
    public static readonly BindableProperty SuffixProperty =
        BindableProperty.Create(nameof(Suffix), typeof(View), typeof(InputGroup), null,
            propertyChanged: (b, o, n) => ((InputGroup)b).UpdateSlots());

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

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public IconName PrefixIcon
    {
        get => (IconName)GetValue(PrefixIconProperty);
        set => SetValue(PrefixIconProperty, value);
    }

    public IconName SuffixIcon
    {
        get => (IconName)GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }

    public string PrefixText
    {
        get => (string)GetValue(PrefixTextProperty);
        set => SetValue(PrefixTextProperty, value);
    }

    public string SuffixText
    {
        get => (string)GetValue(SuffixTextProperty);
        set => SetValue(SuffixTextProperty, value);
    }

    public View? Prefix
    {
        get => (View?)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public View? Suffix
    {
        get => (View?)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler? Completed;

    private readonly Entry _entry;
    private readonly Border _border;
    private readonly ContentView _prefix;
    private readonly ContentView _suffix;
    private bool _isFocused;

    public InputGroup()
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
        _entry.TextChanged += (_, e) =>
        {
            Text = e.NewTextValue;
            TextChanged?.Invoke(this, e);
        };
        _entry.Completed += (_, e) => Completed?.Invoke(this, e);
        _entry.Focused += (_, _) => { _isFocused = true; UpdateVisualState(); };
        _entry.Unfocused += (_, _) => { _isFocused = false; UpdateVisualState(); };

        _prefix = new ContentView { VerticalOptions = LayoutOptions.Center, IsVisible = false };
        _suffix = new ContentView { VerticalOptions = LayoutOptions.Center, IsVisible = false };

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        row.Add(_prefix, 0, 0);
        row.Add(_entry, 1, 0);
        row.Add(_suffix, 2, 0);

        _border = new Border
        {
            Content = row,
            Padding = new Thickness(12, 0),
            HeightRequest = 40,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        // Tapping the padding, the prefix or the suffix focuses the entry too.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _entry.Focus();
        _border.GestureRecognizers.Add(tap);

        Content = _border;
        UpdateVisualState();
    }

    public new void Focus() => _entry.Focus();
    public new void Unfocus() => _entry.Unfocus();

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void UpdateSlots()
    {
        _prefix.Content = Prefix ?? Slot(PrefixIcon, PrefixText);
        _prefix.IsVisible = _prefix.Content != null;
        _suffix.Content = Suffix ?? Slot(SuffixIcon, SuffixText);
        _suffix.IsVisible = _suffix.Content != null;
    }

    // text-muted-foreground: a 16px icon, a text-sm label, or both.
    private static View? Slot(IconName icon, string? text)
    {
        if (icon == IconName.None && string.IsNullOrEmpty(text)) return null;
        var row = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        if (icon != IconName.None)
            row.Children.Add(new Icon { Name = icon, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center });
        if (!string.IsNullOrEmpty(text))
        {
            var label = new Label { Text = text, FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
            label.Token(Label.TextColorProperty, ShellToken.MutedForeground);
            row.Children.Add(label);
        }
        return row;
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
