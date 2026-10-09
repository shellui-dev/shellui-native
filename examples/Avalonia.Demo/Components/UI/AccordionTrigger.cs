using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Section header — flex justify-between py-4 font-medium hover:underline, with a chevron
// that rotates 180° when open. Use Text for a plain title, or put any control inside.
public class AccordionTrigger : ShellTriggerView
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AccordionTrigger, string?>(nameof(Text));

    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<AccordionTrigger, Control?>(nameof(Body));

    private readonly Grid _row;
    private readonly TextBlock _label;
    private readonly Icon _chevron;

    static AccordionTrigger()
    {
        TextProperty.Changed.AddClassHandler<AccordionTrigger>((t, _) => t._label.Text = t.Text ?? string.Empty);
        BodyProperty.Changed.AddClassHandler<AccordionTrigger>((t, e) =>
        {
            t._row.Children.Remove((Control?)e.OldValue ?? t._label);
            t._row.Children.Insert(0, t.Body ?? t._label);
        });
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    [Content]
    public Control? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public AccordionTrigger()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _chevron = new Icon { Kind = IconName.ChevronDown, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(_chevron, 1);

        _row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 16,
            Margin = new Thickness(0, 16),
            Children = { _label, _chevron }
        };

        Child = _row;
        HorizontalAlignment = HorizontalAlignment.Stretch;
    }

    protected override void OnActivated() => this.FindParentOfType<AccordionItem>()?.Toggle();

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _label.TextDecorations = TextDecorations.Underline;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _label.TextDecorations = null;
    }

    internal void SetOpen(bool open, bool animate)
    {
        _chevron.Transitions = animate
            ? new Transitions { new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() } }
            : null;
        _chevron.RenderTransform = TransformOperations.Parse(open ? "rotate(180deg)" : "rotate(0deg)");
    }
}
