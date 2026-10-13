using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PrevNextNavTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "prev-next-nav",
        DisplayName = "Prev Next Nav",
        Description = "Previous and next page links for the foot of a page",
        Category = ComponentCategory.Navigation,
        FilePath = "PrevNextNav.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "previous", "next", "navigation", "pager", "docs" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Previous / next links for the foot of a page — two rounded-md bordered cards side by side,
// each a muted ""Previous"" or ""Next"" over the page's name. A side with no label is left empty.
//   <ui:PrevNextNav PreviousLabel=""Installation"" NextLabel=""Theming""
//                   PreviousClicked=""OnPrevious"" NextClicked=""OnNext"" />
public partial class PrevNextNav : ContentView
{
    public static readonly BindableProperty PreviousLabelProperty =
        BindableProperty.Create(nameof(PreviousLabel), typeof(string), typeof(PrevNextNav), string.Empty,
            propertyChanged: (b, o, n) => ((PrevNextNav)b).Update());

    public static readonly BindableProperty NextLabelProperty =
        BindableProperty.Create(nameof(NextLabel), typeof(string), typeof(PrevNextNav), string.Empty,
            propertyChanged: (b, o, n) => ((PrevNextNav)b).Update());

    // The small text over each label.
    public static readonly BindableProperty PreviousTextProperty =
        BindableProperty.Create(nameof(PreviousText), typeof(string), typeof(PrevNextNav), ""Previous"",
            propertyChanged: (b, o, n) => ((PrevNextNav)b).Update());

    public static readonly BindableProperty NextTextProperty =
        BindableProperty.Create(nameof(NextText), typeof(string), typeof(PrevNextNav), ""Next"",
            propertyChanged: (b, o, n) => ((PrevNextNav)b).Update());

    public string PreviousLabel
    {
        get => (string)GetValue(PreviousLabelProperty);
        set => SetValue(PreviousLabelProperty, value);
    }

    public string NextLabel
    {
        get => (string)GetValue(NextLabelProperty);
        set => SetValue(NextLabelProperty, value);
    }

    public string PreviousText
    {
        get => (string)GetValue(PreviousTextProperty);
        set => SetValue(PreviousTextProperty, value);
    }

    public string NextText
    {
        get => (string)GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public event EventHandler? PreviousClicked;
    public event EventHandler? NextClicked;

    private readonly Border _previous;
    private readonly Border _next;
    private readonly Label _previousText;
    private readonly Label _previousLabel;
    private readonly Label _nextText;
    private readonly Label _nextLabel;

    public PrevNextNav()
    {
        (_previous, _previousText, _previousLabel) = Card(TextAlignment.Start, () => PreviousClicked?.Invoke(this, EventArgs.Empty));
        (_next, _nextText, _nextLabel) = Card(TextAlignment.End, () => NextClicked?.Invoke(this, EventArgs.Empty));

        var grid = new Grid
        {
            ColumnSpacing = 16,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        grid.Add(_previous, 0, 0);
        grid.Add(_next, 1, 0);
        Content = grid;
        Update();
    }

    // rounded-md border p-4, hover:bg-accent: text-sm muted over a font-medium label.
    private (Border Card, Label Text, Label Label) Card(TextAlignment alignment, Action clicked)
    {
        var text = new Label { FontSize = 14, HorizontalTextAlignment = alignment };
        text.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        var label = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = alignment, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.Foreground);
        var card = new Border
        {
            Content = new VerticalStackLayout { Spacing = 2, Children = { text, label } },
            Padding = new Thickness(16),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        card.Token(Border.StrokeProperty, ShellToken.Border);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { if (IsEnabled) card.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent); };
        pointer.PointerExited += (_, _) => { card.ClearValue(VisualElement.BackgroundColorProperty); card.BackgroundColor = Colors.Transparent; };
        card.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsEnabled) { ShellFocus.FocusPressed(card); clicked(); } };
        card.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(card, () => { if (IsEnabled) clicked(); });
        return (card, text, label);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void Update()
    {
        _previousText.Text = PreviousText ?? string.Empty;
        _previousLabel.Text = PreviousLabel ?? string.Empty;
        _previous.IsVisible = !string.IsNullOrEmpty(PreviousLabel);
        SemanticProperties.SetDescription(_previous, $""{PreviousText}: {PreviousLabel}"");
        _nextText.Text = NextText ?? string.Empty;
        _nextLabel.Text = NextLabel ?? string.Empty;
        _next.IsVisible = !string.IsNullOrEmpty(NextLabel);
        SemanticProperties.SetDescription(_next, $""{NextText}: {NextLabel}"");
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Previous / next links for the foot of a page — two rounded-md bordered cards side by side,
// each a muted ""Previous"" or ""Next"" over the page's name. A side with no label is left empty.
// The cards are tab stops (Enter / Space).
//   <ui:PrevNextNav PreviousLabel=""Installation"" NextLabel=""Theming""
//                   PreviousClicked=""OnPrevious"" NextClicked=""OnNext"" />
public class PrevNextNav : Border
{
    public static readonly StyledProperty<string?> PreviousLabelProperty =
        AvaloniaProperty.Register<PrevNextNav, string?>(nameof(PreviousLabel));

    public static readonly StyledProperty<string?> NextLabelProperty =
        AvaloniaProperty.Register<PrevNextNav, string?>(nameof(NextLabel));

    // The small text over each label.
    public static readonly StyledProperty<string?> PreviousTextProperty =
        AvaloniaProperty.Register<PrevNextNav, string?>(nameof(PreviousText), ""Previous"");

    public static readonly StyledProperty<string?> NextTextProperty =
        AvaloniaProperty.Register<PrevNextNav, string?>(nameof(NextText), ""Next"");

    private readonly Border _previous;
    private readonly Border _next;
    private readonly TextBlock _previousText;
    private readonly TextBlock _previousLabel;
    private readonly TextBlock _nextText;
    private readonly TextBlock _nextLabel;

    static PrevNextNav()
    {
        PreviousLabelProperty.Changed.AddClassHandler<PrevNextNav>((n, _) => n.Update());
        NextLabelProperty.Changed.AddClassHandler<PrevNextNav>((n, _) => n.Update());
        PreviousTextProperty.Changed.AddClassHandler<PrevNextNav>((n, _) => n.Update());
        NextTextProperty.Changed.AddClassHandler<PrevNextNav>((n, _) => n.Update());
        IsEnabledProperty.Changed.AddClassHandler<PrevNextNav>((n, _) => n.Opacity = n.IsEnabled ? 1.0 : 0.5);
    }

    public string? PreviousLabel
    {
        get => GetValue(PreviousLabelProperty);
        set => SetValue(PreviousLabelProperty, value);
    }

    public string? NextLabel
    {
        get => GetValue(NextLabelProperty);
        set => SetValue(NextLabelProperty, value);
    }

    public string? PreviousText
    {
        get => GetValue(PreviousTextProperty);
        set => SetValue(PreviousTextProperty, value);
    }

    public string? NextText
    {
        get => GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public event EventHandler? PreviousClicked;
    public event EventHandler? NextClicked;

    public PrevNextNav()
    {
        (_previous, _previousText, _previousLabel) = Card(TextAlignment.Left, () => PreviousClicked?.Invoke(this, EventArgs.Empty));
        (_next, _nextText, _nextLabel) = Card(TextAlignment.Right, () => NextClicked?.Invoke(this, EventArgs.Empty));
        Grid.SetColumn(_next, 1);
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,*""), ColumnSpacing = 16, Children = { _previous, _next } };
        Update();
    }

    // rounded-md border p-4, hover:bg-accent: text-sm muted over a font-medium label.
    private (Border Card, TextBlock Text, TextBlock Label) Card(TextAlignment alignment, Action clicked)
    {
        var text = new TextBlock { FontSize = 14, TextAlignment = alignment };
        text.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        var label = new TextBlock { FontSize = 16, FontWeight = FontWeight.Medium, TextAlignment = alignment, TextTrimming = TextTrimming.CharacterEllipsis };
        label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        var card = new Border
        {
            Child = new StackPanel { Spacing = 2, Children = { text, label } },
            Padding = new Thickness(16),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            // Transparent, not null, so the whole card takes the pointer.
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        card.Token(BorderBrushProperty, ShellToken.Border);
        ShellFocus.Ring(card, card);
        card.PointerEntered += (_, _) => { if (IsEffectivelyEnabled) card.Token(BackgroundProperty, ShellToken.Accent); };
        card.PointerExited += (_, _) =>
        {
            card.ClearToken(BackgroundProperty);
            card.Background = Brushes.Transparent;
        };
        card.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
            clicked();
            e.Handled = true;
        };
        card.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space) || !IsEffectivelyEnabled) return;
            clicked();
            e.Handled = true;
        };
        return (card, text, label);
    }

    private void Update()
    {
        _previousText.Text = PreviousText ?? string.Empty;
        _previousLabel.Text = PreviousLabel ?? string.Empty;
        _previous.IsVisible = !string.IsNullOrEmpty(PreviousLabel);
        AutomationProperties.SetName(_previous, $""{PreviousText}: {PreviousLabel}"");
        _nextText.Text = NextText ?? string.Empty;
        _nextLabel.Text = NextLabel ?? string.Empty;
        _next.IsVisible = !string.IsNullOrEmpty(NextLabel);
        AutomationProperties.SetName(_next, $""{NextText}: {NextLabel}"");
    }
}
"
    };
}
