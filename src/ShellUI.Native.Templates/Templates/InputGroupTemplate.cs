using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class InputGroupTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "input-group",
        DisplayName = "Input Group",
        Description = "Input with an icon, text or view before or after it",
        Category = ComponentCategory.Form,
        FilePath = "InputGroup.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "input", "group", "prefix", "suffix", "addon" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Input with something before or after the text, inside the same border — h-10 rounded-md
// border border-input. The prefix and suffix are an icon, a short text, or any view.
//   <ui:InputGroup PrefixIcon=""Search"" Placeholder=""Search..."" />
//   <ui:InputGroup PrefixText=""https://"" SuffixText="".com"" Placeholder=""example"" />
//   <ui:InputGroup Placeholder=""Amount"">
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
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Input with something before or after the text, inside the same border — h-10 rounded-md
// border border-input. The prefix and suffix are an icon, a short text, or any control.
//   <ui:InputGroup PrefixIcon=""Search"" Placeholder=""Search..."" />
//   <ui:InputGroup PrefixText=""https://"" SuffixText="".com"" Placeholder=""example"" />
//   <ui:InputGroup Placeholder=""Amount"">
//       <ui:InputGroup.Suffix> ...any control... </ui:InputGroup.Suffix>
//   </ui:InputGroup>
public class InputGroup : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<InputGroup, string?>(nameof(Placeholder));

    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<InputGroup, bool>(nameof(IsPassword));

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
        IsPasswordProperty.Changed.AddClassHandler<InputGroup>((g, e) => g._textBox.PasswordChar = (bool)e.NewValue! ? '•' : default);
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
        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""),
            ColumnSpacing = 8,
            Children = { _prefix, _textBox, _suffix }
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
