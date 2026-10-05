using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ToggleGroupTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "toggle-group",
        DisplayName = "Toggle Group",
        Description = "Row of toggles with single or multiple selection",
        Category = ComponentCategory.Form,
        FilePath = "ToggleGroup.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "toggle", "group", "segmented", "selection" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// A row of toggles that work as one control: pick one (default) or several (Multiple).
//   <ui:ToggleGroup Value=""{Binding View}"">
//       <ui:ToggleGroupItem Value=""day"" Text=""Day"" />
//       <ui:ToggleGroupItem Value=""week"" Text=""Week"" />
//   </ui:ToggleGroup>
//   <ui:ToggleGroup Multiple=""True"" Variant=""Outline"" ValuesChanged=""OnFormat""> ... </ui:ToggleGroup>
[ContentProperty(nameof(Items))]
public partial class ToggleGroup : ContentView
{
    public static readonly BindableProperty MultipleProperty =
        BindableProperty.Create(nameof(Multiple), typeof(bool), typeof(ToggleGroup), false,
            propertyChanged: (b, o, n) => ((ToggleGroup)b).Refresh());

    // The pressed item's Value in single mode; null when nothing is pressed.
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(ToggleGroup), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((ToggleGroup)b).OnValueChanged());

    // The pressed items' Values in Multiple mode.
    public static readonly BindableProperty ValuesProperty =
        BindableProperty.Create(nameof(Values), typeof(IList<string>), typeof(ToggleGroup), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((ToggleGroup)b).OnValuesReplaced(o, n));

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ToggleGroupVariant), typeof(ToggleGroup), ToggleGroupVariant.Default,
            propertyChanged: (b, o, n) => ((ToggleGroup)b).Refresh());

    public bool Multiple
    {
        get => (bool)GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IList<string>? Values
    {
        get => (IList<string>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public ToggleGroupVariant Variant
    {
        get => (ToggleGroupVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public IList<IView> Items => _row.Children;

    public event EventHandler<string?>? ValueChanged;
    public event EventHandler<IReadOnlyList<string>>? ValuesChanged;

    private readonly HorizontalStackLayout _row;

    public ToggleGroup()
    {
        _row = new HorizontalStackLayout { Spacing = 4 };
        _row.ChildAdded += (_, _) => Refresh();
        Content = _row;
        HorizontalOptions = LayoutOptions.Start;
        Loaded += (_, _) => Refresh();
    }

    public bool IsPressed(string value) =>
        Multiple ? Values?.Contains(value) == true : Value == value;

    internal void Toggle(string value)
    {
        if (!IsEnabled) return;
        if (Multiple)
        {
            // A new list each time, so a two-way binding sees the change.
            var next = new List<string>(Values ?? Array.Empty<string>());
            if (!next.Remove(value)) next.Add(value);
            Values = next;
        }
        else
        {
            Value = Value == value ? null : value;
        }
    }

    private void OnValueChanged()
    {
        Refresh();
        ValueChanged?.Invoke(this, Value);
    }

    private void OnValuesReplaced(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnValuesMutated;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnValuesMutated;
        OnValuesMutated(this, null);
    }

    private void OnValuesMutated(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Refresh();
        ValuesChanged?.Invoke(this, (Values ?? Array.Empty<string>()).ToList());
    }

    // Items don't track the group's state: the group pushes it to them.
    private void Refresh()
    {
        foreach (var item in _row.Children.OfType<ToggleGroupItem>())
            item.Apply(IsPressed(item.Value), Variant == ToggleGroupVariant.Outline);
    }
}

// One toggle in a ToggleGroup — h-9 min-w-9 rounded-md px-2.5, bg-accent while pressed.
public partial class ToggleGroupItem : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(ToggleGroupItem), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ToggleGroupItem), string.Empty,
            propertyChanged: (b, o, n) => ((ToggleGroupItem)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(ToggleGroupItem), IconName.None,
            propertyChanged: (b, o, n) => ((ToggleGroupItem)b).Update());

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

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private readonly Border _border;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _pressed;
    private bool _outline;
    private bool _hovered;

    public ToggleGroupItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false };
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap
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
            Padding = new Thickness(10, 0),
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _border.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _border;
        Update();
    }

    private void Press()
    {
        if (IsEnabled) this.FindParentOfType<ToggleGroup>()?.Toggle(Value);
    }

    internal void Apply(bool pressed, bool outline)
    {
        _pressed = pressed;
        _outline = outline;
        Update();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;

        _border.StrokeThickness = _outline ? 1 : 0;
        if (_outline) _border.Token(Border.StrokeProperty, ShellToken.Input);

        var foreground = _pressed ? ShellToken.AccentForeground
            : _hovered ? ShellToken.MutedForeground
            : ShellToken.Foreground;
        _label.Token(Label.TextColorProperty, foreground);
        _icon.Token = foreground;

        if (_pressed)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else if (_hovered && IsEnabled)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
    }
}

public enum ToggleGroupVariant { Default, Outline }
"
    };
}
