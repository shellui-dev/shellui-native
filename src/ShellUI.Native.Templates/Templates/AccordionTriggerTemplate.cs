using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AccordionTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "accordion-trigger",
        DisplayName = "Accordion Trigger",
        Description = "Tap-to-toggle handle for the enclosing AccordionItem",
        Category = ComponentCategory.Layout,
        FilePath = "AccordionTrigger.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "layout", "accordion", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Section header — flex justify-between py-4 font-medium hover:underline, with a chevron
// that rotates 180° when open. Use Text for a plain title, or put any view inside.
[ContentProperty(nameof(Body))]
public partial class AccordionTrigger : ShellTriggerView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AccordionTrigger), string.Empty,
            propertyChanged: (b, o, n) => ((AccordionTrigger)b).UpdateBody());

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(AccordionTrigger), null,
            propertyChanged: (b, o, n) => ((AccordionTrigger)b).UpdateBody());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private readonly Grid _row;
    private readonly Label _label;
    private readonly Icon _chevron;

    public AccordionTrigger()
    {
        _label = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);
        _chevron = new Icon { Name = IconName.ChevronDown, Size = 16, Token = ShellToken.MutedForeground };

        _row = new Grid
        {
            Padding = new Thickness(0, 16),
            ColumnSpacing = 16,
            MinimumHeightRequest = 40,
            BackgroundColor = Colors.Transparent,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        _row.Add(_label, 0, 0);
        _row.Add(_chevron, 1, 0);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _label.TextDecorations = TextDecorations.Underline;
        pointer.PointerExited += (_, _) => _label.TextDecorations = TextDecorations.None;
        _row.GestureRecognizers.Add(pointer);

        Content = _row;
    }

    protected override void OnActivated() => this.FindParentOfType<AccordionItem>()?.Toggle();

    internal void SetOpen(bool open, bool animate)
    {
        var target = open ? 180 : 0;
        if (animate) _ = _chevron.RotateToAsync(target, 200, Easing.CubicOut);
        else _chevron.Rotation = target;
    }

    private void UpdateBody()
    {
        var current = _row.Children.FirstOrDefault(c => Grid.GetColumn((BindableObject)c) == 0);
        if (current != null) _row.Children.Remove(current);
        View view = Body ?? _label;
        _label.Text = Text ?? string.Empty;
        _row.Add(view, 0, 0);
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
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

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
            ColumnDefinitions = new ColumnDefinitions(""*,Auto""),
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
        _chevron.RenderTransform = TransformOperations.Parse(open ? ""rotate(180deg)"" : ""rotate(0deg)"");
    }
}
"
    };
}
