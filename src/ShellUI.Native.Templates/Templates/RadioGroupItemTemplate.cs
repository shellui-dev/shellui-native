using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class RadioGroupItemTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "radio-group-item",
        DisplayName = "Radio Group Item",
        Description = "Single radio option",
        Category = ComponentCategory.Form,
        FilePath = "RadioGroupItem.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "form", "radio", "option" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Radio option — h-4 w-4 rounded-full border border-primary; checked shows a filled dot.
public partial class RadioGroupItem : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(RadioGroupItem), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(RadioGroupItem), string.Empty,
            propertyChanged: (b, o, n) => ((RadioGroupItem)b)._label.Text = n as string ?? string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsChecked { get; private set; }

    private readonly Border _ring;
    private readonly Border _dot;
    private readonly Label _label;

    public RadioGroupItem()
    {
        _dot = new Border
        {
            WidthRequest = 8,
            HeightRequest = 8,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Scale = 0
        };
        _dot.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);

        _ring = new Border
        {
            Content = _dot,
            WidthRequest = 16,
            HeightRequest = 16,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            VerticalOptions = LayoutOptions.Center
        };
        _ring.Token(Border.StrokeProperty, ShellToken.Primary);

        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);

        var row = new HorizontalStackLayout { Spacing = 8, Children = { _ring, _label } };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); Select(); };
        row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) Select(); });

        Content = row;
        HorizontalOptions = LayoutOptions.Start;
    }

    private void Select() => this.FindParentOfType<RadioGroup>()?.SetValue(Value);

    internal void SetChecked(bool isChecked)
    {
        if (IsChecked == isChecked && _dot.Scale == (isChecked ? 1 : 0)) return;
        IsChecked = isChecked;
        _ = _dot.ScaleToAsync(isChecked ? 1 : 0, 120, Easing.CubicOut);
    }
}
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

namespace YourProjectNamespace.Components.UI;

// Radio option — h-4 w-4 rounded-full border border-primary; checked shows a filled dot.
public class RadioGroupItem : Border
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<RadioGroupItem, string?>(nameof(Value));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<RadioGroupItem, string?>(nameof(Text));

    private readonly Border _ring;
    private readonly Border _dot;
    private readonly TextBlock _label;

    static RadioGroupItem()
    {
        TextProperty.Changed.AddClassHandler<RadioGroupItem>((i, e) => i._label.Text = (string?)e.NewValue ?? string.Empty);
        IsEnabledProperty.Changed.AddClassHandler<RadioGroupItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsChecked { get; private set; }

    public RadioGroupItem()
    {
        _dot = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransform = TransformOperations.Parse(""scale(0)"")
        };
        _dot.Token(BackgroundProperty, ShellToken.Primary);
        _dot.Transitions = new Transitions
        {
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(120), Easing = new CubicEaseOut() }
        };

        _ring = new Border
        {
            Child = _dot,
            Width = 16,
            Height = 16,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center
        };
        _ring.Token(BorderBrushProperty, ShellToken.Primary);

        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _ring, _label } };
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, _ring);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        Select();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key != Key.Space) return;
        Select();
        e.Handled = true;
    }

    private void Select() => this.FindParentOfType<RadioGroup>()?.SetValue(Value);

    internal void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
        _dot.RenderTransform = TransformOperations.Parse(isChecked ? ""scale(1)"" : ""scale(0)"");
    }
}
"
    };
}
