using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ToggleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "toggle",
        DisplayName = "Toggle",
        Description = "Two-state button that stays pressed",
        Category = ComponentCategory.Form,
        FilePath = "Toggle.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "toggle", "button", "pressed" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Two-state button — h-10 px-3 rounded-md text-sm font-medium; hover:bg-muted, and
// bg-accent text-accent-foreground while pressed. Variant Outline adds border-input.
// Usage: <ui:Toggle Icon=""Star"" IsPressed=""{Binding IsFavorite}"" />  <ui:Toggle Text=""Bold"" Variant=""Outline"" />
public partial class Toggle : ContentView, IShellFocusable
{
    public static readonly BindableProperty IsPressedProperty =
        BindableProperty.Create(nameof(IsPressed), typeof(bool), typeof(Toggle), false, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Toggle)b).OnPressedChanged());

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Toggle), string.Empty,
            propertyChanged: (b, o, n) => ((Toggle)b).UpdateVisualState());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(Toggle), IconName.None,
            propertyChanged: (b, o, n) => ((Toggle)b).UpdateVisualState());

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ToggleVariant), typeof(Toggle), ToggleVariant.Default,
            propertyChanged: (b, o, n) => ((Toggle)b).UpdateVisualState());

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ToggleSize), typeof(Toggle), ToggleSize.Default,
            propertyChanged: (b, o, n) => ((Toggle)b).UpdateVisualState());

    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
        set => SetValue(IsPressedProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ToggleVariant Variant
    {
        get => (ToggleVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ToggleSize Size
    {
        get => (ToggleSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public event EventHandler<bool>? PressedChanged;

    private readonly Border _border;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _hovered;

    public Toggle()
    {
        _icon = new Icon { Size = 16, IsVisible = false };
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };
        _border = new Border
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { _icon, _label }
            },
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; UpdateVisualState(); };
        pointer.PointerExited += (_, _) => { _hovered = false; UpdateVisualState(); };
        _border.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        UpdateVisualState();
    }

    private void Press()
    {
        if (IsEnabled) IsPressed = !IsPressed;
    }

    private void OnPressedChanged()
    {
        UpdateVisualState();
        PressedChanged?.Invoke(this, IsPressed);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void UpdateVisualState()
    {
        var (height, padding) = Size switch
        {
            ToggleSize.Sm => (36.0, 10.0),
            ToggleSize.Lg => (44.0, 20.0),
            _ => (40.0, 12.0)
        };
        _border.HeightRequest = height;
        _border.MinimumWidthRequest = height;
        _border.Padding = new Thickness(padding, 0);
        _border.StrokeThickness = Variant == ToggleVariant.Outline ? 1 : 0;
        if (Variant == ToggleVariant.Outline) _border.Token(Border.StrokeProperty, ShellToken.Input);

        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;

        var foreground = IsPressed ? ShellToken.AccentForeground
            : _hovered ? ShellToken.MutedForeground
            : ShellToken.Foreground;
        _label.Token(Label.TextColorProperty, foreground);
        _icon.Token = foreground;

        if (IsPressed)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else if (_hovered && IsEnabled)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
    }
}

public enum ToggleVariant { Default, Outline }
public enum ToggleSize { Sm, Default, Lg }
"
    };
}
