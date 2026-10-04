using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SelectTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "select",
        DisplayName = "Select",
        Description = "Dropdown select / picker",
        Category = ComponentCategory.Form,
        FilePath = "Select.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "form", "select", "picker", "dropdown" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Select — shadcn-style trigger (h-10 rounded-md border, chevrons icon) and a list that floats
// in the page layer, with a check on the selected item. Clicking outside closes it. Custom-drawn, so it looks the same on every platform.
// Usage: <ui:Select Placeholder=""Pick a country"" /> then set ItemsSource in code or by binding.
public partial class Select : ContentView, IShellPopup
{
    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Select),
            -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(Select),
            null, propertyChanged: (b, o, n) => ((Select)b).RebuildItems());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Select),
            ""Select..."", propertyChanged: (b, o, n) => ((Select)b).UpdateTrigger());

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public IList<string>? ItemsSource
    {
        get => (IList<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string? SelectedItem => SelectedIndex >= 0 && ItemsSource != null && SelectedIndex < ItemsSource.Count
        ? ItemsSource[SelectedIndex] : null;

    public bool IsOpen { get; private set; }

    public event EventHandler? SelectedIndexChanged;

    private ShellPopupHandle? _handle;
    private readonly Border _trigger;
    private readonly Label _value;
    private readonly Border _panel;
    private readonly VerticalStackLayout _list;
    private readonly ScrollView _scroll;

    private const double ItemHeight = 32;
    private const double MaxListHeight = 280;

    public Select()
    {
        _value = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        var chevrons = new Icon { Name = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground };
        var triggerRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        triggerRow.Add(_value, 0, 0);
        triggerRow.Add(chevrons, 1, 0);

        _trigger = new Border
        {
            Content = triggerRow,
            HeightRequest = 40,
            Padding = new Thickness(12, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); SetOpen(!IsOpen); };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _list = new VerticalStackLayout { Spacing = 0 };
        _scroll = new ScrollView { Content = _list };
        _panel = new Border
        {
            Content = _scroll,
            Padding = new Thickness(4),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);

        Content = _trigger;
        UpdateTrigger();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions
            {
                MatchAnchorWidth = true,
                Owner = this,
                OnDismiss = Close
            });
        }
        else
        {
            ShellPopups.Closed(this);
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }

    public void Close() => SetOpen(false);

    private static void OnSelectedIndexChanged(BindableObject b, object o, object n)
    {
        var select = (Select)b;
        select.UpdateTrigger();
        select.UpdateItemStates();
        select.SelectedIndexChanged?.Invoke(select, EventArgs.Empty);
    }

    private void UpdateTrigger()
    {
        var item = SelectedItem;
        _value.Text = item ?? Placeholder;
        _value.Token(Label.TextColorProperty, item != null ? ShellToken.Foreground : ShellToken.MutedForeground);
    }

    private void RebuildItems()
    {
        _list.Children.Clear();
        var items = ItemsSource;
        // Explicit height: a ScrollView sizes to its content on some platforms and stretches to
        // the available space on others.
        _scroll.HeightRequest = Math.Min((items?.Count ?? 0) * ItemHeight, MaxListHeight);
        if (items == null) return;
        for (var i = 0; i < items.Count; i++)
            _list.Children.Add(CreateItem(items[i], i));
        UpdateTrigger();
        UpdateItemStates();
    }

    private View CreateItem(string text, int index)
    {
        var label = new Label { Text = text, FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        var check = new Icon { Name = IconName.Check, Size = 16, IsVisible = false };
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(label, 0, 0);
        row.Add(check, 1, 0);

        var item = new Border
        {
            Content = row,
            HeightRequest = ItemHeight,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => item.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { item.ClearValue(VisualElement.BackgroundColorProperty); item.BackgroundColor = Colors.Transparent; };
        item.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { SelectedIndex = index; SetOpen(false); };
        item.GestureRecognizers.Add(tap);
        return item;
    }

    private void UpdateItemStates()
    {
        for (var i = 0; i < _list.Children.Count; i++)
        {
            if (_list.Children[i] is Border { Content: Grid row } && row.Children.Count > 1 && row.Children[1] is Icon check)
                check.IsVisible = i == SelectedIndex;
        }
    }
}
"
    };
}
