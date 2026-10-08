using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using AvaloniaDemo.Components.UI.Variants;

namespace AvaloniaDemo.Components.UI;

// Button with ShellUI variants, sizes, optional icon and loading state.
// Sizes to its content like an inline-flex button; set HorizontalAlignment="Stretch" for a block button.
// Usage: <ui:Button Text="Save" Icon="Check" Variant="Outline" Clicked="OnSave" />
public class Button : Border, IShellFocusable
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
        RenderTransform = TransformOperations.Parse("scale(1)");
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
        if (!CanClick) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        // Inside a DialogTrigger, DropdownTrigger, ... the click also activates the trigger.
        ShellTriggerView.ActivateAncestor(this);
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
        RenderTransform = TransformOperations.Parse("scale(0.97)");
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_pressed) return;
        _pressed = false;
        RenderTransform = TransformOperations.Parse("scale(1)");
        // Releasing outside the button cancels the click, as with native buttons.
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this))) Press();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _pressed = false;
        RenderTransform = TransformOperations.Parse("scale(1)");
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
