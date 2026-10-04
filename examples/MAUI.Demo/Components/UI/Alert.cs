using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Callout with icon, title and message — rounded-lg border p-4, icon + text tinted per variant.
public partial class Alert : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Alert),
            string.Empty, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(Alert),
            string.Empty, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(AlertVariant), typeof(Alert),
            AlertVariant.Default, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _container;
    private readonly Icon _icon;
    private readonly Label _titleLabel;
    private readonly Label _messageLabel;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public AlertVariant Variant
    {
        get => (AlertVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Alert()
    {
        _icon = new Icon { Size = 16, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 2, 0, 0) };
        _titleLabel = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold };
        _messageLabel = new Label { FontSize = 14 };

        var text = new VerticalStackLayout { Spacing = 4, Children = { _titleLabel, _messageLabel } };
        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        row.Add(_icon, 0, 0);
        row.Add(text, 1, 0);

        _container = new Border
        {
            Content = row,
            Padding = new Thickness(16, 12),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        _container.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);

        Content = _container;
        UpdateVisualState();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Alert)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        var (icon, accent) = Variant switch
        {
            AlertVariant.Destructive => (IconName.CircleAlert, ShellToken.Destructive),
            AlertVariant.Success => (IconName.CircleCheck, ShellToken.Success),
            AlertVariant.Warning => (IconName.TriangleAlert, ShellToken.Warning),
            AlertVariant.Info => (IconName.Info, ShellToken.Info),
            _ => (IconName.Info, ShellToken.Foreground)
        };

        _icon.Name = icon;
        _icon.Token = accent;
        _titleLabel.Text = Title ?? string.Empty;
        _titleLabel.IsVisible = !string.IsNullOrEmpty(Title);
        _messageLabel.Text = Message ?? string.Empty;
        _messageLabel.IsVisible = !string.IsNullOrEmpty(Message);

        // Tinted variants color the title; the message stays readable in the muted foreground.
        _titleLabel.Token(Label.TextColorProperty, accent);
        _messageLabel.Token(Label.TextColorProperty,
            Variant == AlertVariant.Default ? ShellToken.MutedForeground : ShellToken.Foreground);
        _container.Token(Border.StrokeProperty, Variant == AlertVariant.Default ? ShellToken.Border : accent);
    }
}

public enum AlertVariant
{
    Default,
    Destructive,
    Success,
    Warning,
    Info
}
