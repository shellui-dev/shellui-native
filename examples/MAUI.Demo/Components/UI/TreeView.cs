using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Tree of expandable rows — rounded-md px-2 py-1.5 text-sm rows, hover:bg-muted, bg-accent when
// selected; children are indented behind a hairline. Tapping a row selects it and, when it has
// children, expands or collapses it.
//   <ui:TreeView SelectedValue="{Binding Path}">
//       <ui:TreeViewItem Text="src" Icon="Folder" IsExpanded="True">
//           <ui:TreeViewItem Text="App.xaml" Icon="File" />
//       </ui:TreeViewItem>
//   </ui:TreeView>
[ContentProperty(nameof(Items))]
public partial class TreeView : ContentView
{
    // The selected item's Value (its Text when no Value is set).
    public static readonly BindableProperty SelectedValueProperty =
        BindableProperty.Create(nameof(SelectedValue), typeof(string), typeof(TreeView), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((TreeView)b).OnSelectedValueChanged());

    public string? SelectedValue
    {
        get => (string?)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public IList<IView> Items => _stack.Children;

    public event EventHandler<string?>? SelectedValueChanged;

    private readonly VerticalStackLayout _stack;

    public TreeView()
    {
        _stack = new VerticalStackLayout { Spacing = 2 };
        Content = _stack;
        Loaded += (_, _) => Refresh();
    }

    private void OnSelectedValueChanged()
    {
        Refresh();
        SelectedValueChanged?.Invoke(this, SelectedValue);
    }

    // Items don't track the selection: the tree pushes it to every item under it.
    private void Refresh()
    {
        foreach (var item in this.FindDescendantsOfType<TreeViewItem>())
            item.ApplySelection(SelectedValue);
    }
}

// One row of a TreeView. Nested TreeViewItems become its children.
[ContentProperty(nameof(Items))]
public partial class TreeViewItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TreeViewItem), string.Empty,
            propertyChanged: (b, o, n) => ((TreeViewItem)b).Update());

    // Identifies the item for selection. Defaults to Text.
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TreeViewItem), null);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(TreeViewItem), IconName.None,
            propertyChanged: (b, o, n) => ((TreeViewItem)b).Update());

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(TreeViewItem), false, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((TreeViewItem)b).OnExpandedChanged());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public IList<IView> Items => _children.Children;

    public string EffectiveValue => Value ?? Text ?? string.Empty;

    private readonly Border _row;
    private readonly Icon _chevron;
    private readonly Icon _icon;
    private readonly Label _label;
    private readonly Grid _childrenHost;
    private readonly VerticalStackLayout _children;
    private bool _selected;
    private bool _hovered;

    public TreeViewItem()
    {
        _chevron = new Icon { Name = IconName.ChevronRight, Size = 16, Token = ShellToken.MutedForeground };
        _icon = new Icon { Size = 16, Token = ShellToken.MutedForeground, IsVisible = false };
        _label = new Label
        {
            FontSize = 14,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        // The chevron keeps its 16px slot on leaves, so labels line up within a level.
        var slot = new Grid { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center, Children = { _chevron } };
        var content = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)
            }
        };
        content.Add(slot, 0, 0);
        content.Add(_icon, 1, 0);
        content.Add(_label, 2, 0);

        _row = new Border
        {
            Content = content,
            HeightRequest = 32,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Paint(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Paint(); };
        _row.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        // ml-4 border-l pl-2: a hairline under the parent's chevron, children to its right.
        var guide = new BoxView { WidthRequest = 1, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(16, 0, 0, 0) };
        guide.Token(BoxView.ColorProperty, ShellToken.Border);
        _children = new VerticalStackLayout { Spacing = 2, Margin = new Thickness(25, 0, 0, 0) };
        _children.ChildAdded += (_, _) => Update();
        _children.ChildRemoved += (_, _) => Update();
        _childrenHost = new Grid { Margin = new Thickness(0, 2, 0, 0), IsVisible = false, Children = { guide, _children } };

        Content = new VerticalStackLayout { Spacing = 0, Children = { _row, _childrenHost } };
        Update();
    }

    private bool HasChildren => _children.Children.Count > 0;

    private void Press()
    {
        if (!IsEnabled) return;
        if (HasChildren) IsExpanded = !IsExpanded;
        if (this.FindParentOfType<TreeView>() is { } tree) tree.SelectedValue = EffectiveValue;
    }

    internal void ApplySelection(string? selected)
    {
        _selected = selected != null && selected == EffectiveValue;
        Paint();
    }

    private void OnExpandedChanged() => Update();

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _chevron.IsVisible = HasChildren;
        // A second icon rather than a rotation: a view rotated before its first layout pivots
        // around its corner on Android and ends up outside its slot.
        _chevron.Name = IsExpanded ? IconName.ChevronDown : IconName.ChevronRight;
        _childrenHost.IsVisible = HasChildren && IsExpanded;
        Paint();
    }

    private void Paint()
    {
        _label.FontAttributes = _selected ? FontAttributes.Bold : FontAttributes.None;
        _label.Token(Label.TextColorProperty, _selected ? ShellToken.AccentForeground : ShellToken.Foreground);
        if (_selected)
            _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else if (_hovered && IsEnabled)
            _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        else
        {
            _row.ClearValue(VisualElement.BackgroundColorProperty);
            _row.BackgroundColor = Colors.Transparent;
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
