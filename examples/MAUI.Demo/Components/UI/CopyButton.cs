using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Copies Text to the clipboard — a ghost icon button (h-9 w-9 rounded-md, hover:bg-accent) whose
// icon turns into a check for two seconds after copying. Label adds text next to the icon.
// Usage: <ui:CopyButton Text="shellui-native add button" />
public partial class CopyButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(CopyButton), string.Empty,
            propertyChanged: (b, o, n) => ((CopyButton)b).Update());

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(CopyButton), string.Empty,
            propertyChanged: (b, o, n) => ((CopyButton)b).Update());

    // The text placed on the clipboard.
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event EventHandler? Copied;

    private readonly Border _border;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _copied;
    private bool _hovered;
    private int _version;

    public CopyButton()
    {
        _icon = new Icon { Name = IconName.Copy, Size = 16 };
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap,
            IsVisible = false
        };
        _border = new Border
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { _icon, _label }
            },
            HeightRequest = 36,
            MinimumWidthRequest = 36,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _border.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); _ = CopyAsync(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => _ = CopyAsync());

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        Update();
    }

    public async Task CopyAsync()
    {
        if (!IsEnabled || string.IsNullOrEmpty(Text)) return;
        try
        {
            await Clipboard.Default.SetTextAsync(Text);
        }
        catch (Exception)
        {
            // No clipboard access (e.g. a locked-down device): leave the icon as it is.
            return;
        }

        var version = ++_version;
        _copied = true;
        Update();
        Copied?.Invoke(this, EventArgs.Empty);
        await Task.Delay(2000);
        if (version != _version) return;
        _copied = false;
        Update();
    }

    private void Update()
    {
        var hasLabel = !string.IsNullOrEmpty(Label);
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = hasLabel;
        _border.Padding = new Thickness(hasLabel ? 12 : 0, 0);

        _icon.Name = _copied ? IconName.Check : IconName.Copy;
        var foreground = _copied ? ShellToken.Success : _hovered ? ShellToken.AccentForeground : ShellToken.Foreground;
        _icon.Token = foreground;
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, _hovered ? ShellToken.AccentForeground : ShellToken.Foreground);

        if (_hovered && IsEnabled)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
        Opacity = IsEnabled && !string.IsNullOrEmpty(Text) ? 1.0 : 0.5;
        SemanticProperties.SetDescription(this, _copied ? "Copied" : "Copy to clipboard");
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _border != null) Update();
    }
}
