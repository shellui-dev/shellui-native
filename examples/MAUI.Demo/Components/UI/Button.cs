using Microsoft.Maui.Controls.Shapes;
using MAUI.Demo.Components.UI.Variants;

namespace MAUI.Demo.Components.UI;

// Button with ShellUI variants, sizes, optional icon and loading state.
// Sizes to its content like an inline-flex button; set HorizontalOptions="Fill" for a block button.
// Usage: <ui:Button Text="Save" Icon="Check" Variant="Outline" Clicked="OnSave" />
public partial class Button : ContentView, IShellFocusable
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ButtonVariant), typeof(Button),
            ButtonVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ButtonSize), typeof(Button),
            ButtonSize.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(Button),
            false, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Button),
            string.Empty, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(Button),
            IconName.None, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IconPositionProperty =
        BindableProperty.Create(nameof(IconPosition), typeof(IconPosition), typeof(Button),
            IconPosition.Left, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _border;
    private readonly HorizontalStackLayout _row;
    private readonly Icon _spinner;
    private readonly Icon _icon;
    private readonly Label _label;
    private ButtonStyle _style = new();
    private bool _hovered;

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ButtonSize Size
    {
        get => (ButtonSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
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

    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    public event EventHandler? Clicked;

    public Button()
    {
        _spinner = new Icon { Name = IconName.LoaderCircle, IsVisible = false };
        _icon = new Icon { IsVisible = false };
        _label = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap,
            FontAttributes = FontAttributes.Bold
        };
        _row = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _border = new Border
        {
            Content = _row,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; ApplyHover(); };
        pointer.PointerExited += (_, _) => { _hovered = false; ApplyHover(); _border.Scale = 1; };
        pointer.PointerPressed += (_, _) => { if (CanClick) _ = _border.ScaleToAsync(0.97, 60, Easing.CubicOut); };
        pointer.PointerReleased += (_, _) => _ = _border.ScaleToAsync(1, 90, Easing.CubicOut);
        _border.GestureRecognizers.Add(pointer);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        UpdateVisualState();
    }

    private bool CanClick => IsEnabled && !IsLoading;

    private void Press()
    {
        if (!CanClick) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        // Inside a DialogTrigger / CollapsibleTrigger / ... the click also activates the trigger.
        ShellTriggerView.ActivateAncestor(this);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Button)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        _style = ButtonVariants.GetStyle(Variant, Size);

        _border.HeightRequest = _style.Height;
        _border.WidthRequest = _style.Width;
        _border.Padding = _style.Padding;
        _border.StrokeShape = new RoundRectangle { CornerRadius = _style.CornerRadius };
        _border.StrokeThickness = _style.Border.HasValue ? 1 : 0;
        if (_style.Border.HasValue)
            _border.Token(Border.StrokeProperty, _style.Border.Value);

        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _label.FontSize = _style.FontSize;
        _label.Token(Label.TextColorProperty, _style.Foreground);

        _icon.Name = Icon;
        _icon.Size = _style.IconSize;
        _icon.Token = _style.Foreground;
        _icon.IsVisible = Icon != IconName.None && !IsLoading;

        _spinner.Size = _style.IconSize;
        _spinner.Token = _style.Foreground;
        _spinner.IsVisible = IsLoading;
        if (IsLoading) StartSpinner(); else StopSpinner();

        _row.Children.Clear();
        _row.Children.Add(_spinner);
        if (IconPosition == IconPosition.Left) { _row.Children.Add(_icon); _row.Children.Add(_label); }
        else { _row.Children.Add(_label); _row.Children.Add(_icon); }

        ApplyHover();
    }

    private void ApplyHover()
    {
        var hover = _hovered && CanClick;
        var background = hover && _style.HoverBackground.HasValue ? _style.HoverBackground : _style.Background;
        if (background.HasValue)
            _border.Token(VisualElement.BackgroundColorProperty, background.Value);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
        _border.Opacity = hover ? _style.HoverOpacity : 1.0;
        _label.TextDecorations = hover && _style.UnderlineOnHover ? TextDecorations.Underline : TextDecorations.None;
    }

    private void StartSpinner()
    {
        if (this.AnimationIsRunning("ButtonSpin")) return;
        new Animation(v => _spinner.Rotation = v, 0, 360)
            .Commit(this, "ButtonSpin", 16, 800, Easing.Linear, repeat: () => IsLoading);
    }

    private void StopSpinner()
    {
        this.AbortAnimation("ButtonSpin");
        _spinner.Rotation = 0;
    }
}

public enum IconPosition { Left, Right }
