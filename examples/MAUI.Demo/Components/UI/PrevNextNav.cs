using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Previous / next links for the foot of a page — two rounded-md bordered cards side by side,
// each a muted "Previous" or "Next" over the page's name. A side with no label is left empty.
//   <ui:PrevNextNav PreviousLabel="Installation" NextLabel="Theming"
//                   PreviousClicked="OnPrevious" NextClicked="OnNext" />
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
        BindableProperty.Create(nameof(PreviousText), typeof(string), typeof(PrevNextNav), "Previous",
            propertyChanged: (b, o, n) => ((PrevNextNav)b).Update());

    public static readonly BindableProperty NextTextProperty =
        BindableProperty.Create(nameof(NextText), typeof(string), typeof(PrevNextNav), "Next",
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
        SemanticProperties.SetDescription(_previous, $"{PreviousText}: {PreviousLabel}");
        _nextText.Text = NextText ?? string.Empty;
        _nextLabel.Text = NextLabel ?? string.Empty;
        _next.IsVisible = !string.IsNullOrEmpty(NextLabel);
        SemanticProperties.SetDescription(_next, $"{NextText}: {NextLabel}");
    }
}
