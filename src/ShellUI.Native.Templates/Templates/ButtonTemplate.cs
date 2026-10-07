using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Button component template for MAUI
public static class ButtonTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "button",
        DisplayName = "Button",
        Description = "Interactive button component with multiple variants and sizes",
        Category = ComponentCategory.Form,
        FilePath = "Button.cs",
        Dependencies = new List<string> { "shell", "icon", "button-variants" },
        Variants = new List<string> { "default", "destructive", "outline", "secondary", "ghost" },
        Tags = new List<string> { "form", "input", "interactive", "button", "action" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Button with ShellUI variants, sizes, optional icon and loading state.
// Sizes to its content like an inline-flex button; set HorizontalOptions=""Fill"" for a block button.
// Usage: <ui:Button Text=""Save"" Icon=""Check"" Variant=""Outline"" Clicked=""OnSave"" />
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
        if (this.AnimationIsRunning(""ButtonSpin"")) return;
        new Animation(v => _spinner.Rotation = v, 0, 360)
            .Commit(this, ""ButtonSpin"", 16, 800, Easing.Linear, repeat: () => IsLoading);
    }

    private void StopSpinner()
    {
        this.AbortAnimation(""ButtonSpin"");
        _spinner.Rotation = 0;
    }
}

public enum IconPosition { Left, Right }
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Button with ShellUI variants, sizes, optional icon and loading state.
// Sizes to its content like an inline-flex button; set HorizontalAlignment=""Stretch"" for a block button.
// Usage: <ui:Button Text=""Save"" Icon=""Check"" Variant=""Outline"" Clicked=""OnSave"" />
public class Button : Border
{
    public static readonly StyledProperty<ButtonVariant> VariantProperty =
        AvaloniaProperty.Register<Button, ButtonVariant>(nameof(Variant));

    public static readonly StyledProperty<ButtonSize> SizeProperty =
        AvaloniaProperty.Register<Button, ButtonSize>(nameof(Size));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Button, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<Button, IconName>(nameof(Icon));

    public static readonly StyledProperty<IconPosition> IconPositionProperty =
        AvaloniaProperty.Register<Button, IconPosition>(nameof(IconPosition));

    private readonly StackPanel _row;
    private readonly Icon _spinner;
    private readonly RotateTransform _spin = new();
    private readonly Icon _icon;
    private readonly TextBlock _label;
    private ButtonStyle _style = new();
    private DispatcherTimer? _spinning;
    private bool _hovered;
    private bool _pressed;

    static Button()
    {
        VariantProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        SizeProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        IsLoadingProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        TextProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        IconProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        IconPositionProperty.Changed.AddClassHandler<Button>((b, _) => b.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<Button>((b, _) => b.Opacity = b.IsEnabled ? 1.0 : 0.5);
    }

    public ButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ButtonSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconPosition IconPosition
    {
        get => GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    public event EventHandler? Clicked;

    public Button()
    {
        _spinner = new Icon { Kind = IconName.LoaderCircle, IsVisible = false, RenderTransform = _spin };
        _icon = new Icon { IsVisible = false };
        _label = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeight.Medium };
        _row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Child = _row;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        Cursor = new Cursor(StandardCursorType.Hand);
        RenderTransform = TransformOperations.Parse(""scale(1)"");
        Transitions = new Transitions
        {
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(90), Easing = new CubicEaseOut() }
        };
        ShellFocus.Ring(this, this);
        UpdateVisualState();
    }

    private bool CanClick => IsEffectivelyEnabled && !IsLoading;

    // Clicks in code, as a pointer click or Enter/Space would.
    public void Press()
    {
        if (CanClick) Clicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _hovered = true;
        ApplyHover();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _hovered = false;
        ApplyHover();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!CanClick || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _pressed = true;
        RenderTransform = TransformOperations.Parse(""scale(0.97)"");
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_pressed) return;
        _pressed = false;
        RenderTransform = TransformOperations.Parse(""scale(1)"");
        // Releasing outside the button cancels the click, as with native buttons.
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this))) Press();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _pressed = false;
        RenderTransform = TransformOperations.Parse(""scale(1)"");
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Press();
        e.Handled = true;
    }

    private void UpdateVisualState()
    {
        _style = ButtonVariants.GetStyle(Variant, Size);

        Height = _style.Height;
        Width = _style.Width;
        Padding = _style.Padding;
        CornerRadius = new CornerRadius(_style.CornerRadius);
        BorderThickness = new Thickness(_style.Border.HasValue ? 1 : 0);
        if (_style.Border.HasValue)
            this.Token(BorderBrushProperty, _style.Border.Value);

        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _label.FontSize = _style.FontSize;
        _label.Token(TextBlock.ForegroundProperty, _style.Foreground);

        _icon.Kind = Icon;
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
            this.Token(BackgroundProperty, background.Value);
        else
        {
            // Transparent, not null, so the whole button stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = !IsEnabled ? 0.5 : hover ? _style.HoverOpacity : 1.0;
        _label.TextDecorations = hover && _style.UnderlineOnHover ? TextDecorations.Underline : null;
    }

    // One turn per 800 ms, advanced once per frame.
    private void StartSpinner()
    {
        if (_spinning != null) return;
        _spinning = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render,
            (_, _) => _spin.Angle = (_spin.Angle + 360 * 16 / 800.0) % 360);
        _spinning.Start();
    }

    private void StopSpinner()
    {
        _spinning?.Stop();
        _spinning = null;
        _spin.Angle = 0;
    }
}

public enum IconPosition { Left, Right }
"
    };
}
