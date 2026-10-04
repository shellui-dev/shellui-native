using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional leading icon.
public partial class DropdownItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(DropdownItem), string.Empty,
            propertyChanged: (b, o, n) => ((DropdownItem)b)._label.Text = n as string ?? string.Empty);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(DropdownItem), IconName.None,
            propertyChanged: (b, o, n) => ((DropdownItem)b).UpdateIcon());

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

    public event EventHandler? Clicked;

    private readonly Border _row;
    private readonly Icon _icon;
    private readonly Label _label;

    public DropdownItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false };
        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);

        _row = new Border
        {
            Content = new HorizontalStackLayout { Spacing = 8, Children = { _icon, _label } },
            HeightRequest = 32,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { _row.ClearValue(VisualElement.BackgroundColorProperty); _row.BackgroundColor = Colors.Transparent; };
        _row.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Press();
        _row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _row;
    }

    private void Press()
    {
        if (!IsEnabled) return;
        this.FindParentOfType<Dropdown>()?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateIcon()
    {
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
    }
}
