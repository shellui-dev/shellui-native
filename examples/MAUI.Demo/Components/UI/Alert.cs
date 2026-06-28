using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Alert component for notifications and feedback
public partial class Alert : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Alert), 
            string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(Alert), 
            string.Empty, propertyChanged: OnMessageChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(AlertVariant), typeof(Alert), 
            AlertVariant.Default, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _container;
    private readonly Label _titleLabel;
    private readonly Label _messageLabel;
    private readonly VerticalStackLayout _contentStack;

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
        _titleLabel = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 0, 0, 4)
        };

        _messageLabel = new Label
        {
            FontSize = 14
        };

        _contentStack = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(16),
            Children = { _titleLabel, _messageLabel }
        };

        _container = new Border
        {
            Content = _contentStack,
            Padding = new Thickness(0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 6 }
        };

        Content = _container;
        UpdateVisualState();
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Alert alert)
        {
            var text = newValue as string ?? string.Empty;
            alert._titleLabel.Text = text;
            alert._titleLabel.IsVisible = !string.IsNullOrEmpty(text);
        }
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Alert alert)
        {
            var text = newValue as string ?? string.Empty;
            alert._messageLabel.Text = text;
            alert._messageLabel.IsVisible = !string.IsNullOrEmpty(text);
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Alert alert)
            alert.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme - variant color schemes
        var (bg, fg, border, titleColor) = Variant switch
        {
            AlertVariant.Default => (
                Color.FromArgb("#F3F4F6"),
                Color.FromArgb("#374151"),
                Color.FromArgb("#E5E7EB"),
                Color.FromArgb("#1F2937")
            ),
            AlertVariant.Destructive => (
                Color.FromArgb("#FEF2F2"),
                Color.FromArgb("#991B1B"),
                Color.FromArgb("#FECACA"),
                Color.FromArgb("#DC2626")
            ),
            AlertVariant.Success => (
                Color.FromArgb("#F0FDF4"),
                Color.FromArgb("#166534"),
                Color.FromArgb("#BBF7D0"),
                Color.FromArgb("#22C55E")
            ),
            AlertVariant.Warning => (
                Color.FromArgb("#FFFBEB"),
                Color.FromArgb("#92400E"),
                Color.FromArgb("#FDE68A"),
                Color.FromArgb("#F59E0B")
            ),
            AlertVariant.Info => (
                Color.FromArgb("#EFF6FF"),
                Color.FromArgb("#1E40AF"),
                Color.FromArgb("#BFDBFE"),
                Color.FromArgb("#3B82F6")
            ),
            _ => (
                Color.FromArgb("#F3F4F6"),
                Color.FromArgb("#374151"),
                Color.FromArgb("#E5E7EB"),
                Color.FromArgb("#1F2937")
            )
        };

        _container.BackgroundColor = bg;
        _container.Stroke = border;
        _titleLabel.TextColor = titleColor;
        _messageLabel.TextColor = fg;
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
