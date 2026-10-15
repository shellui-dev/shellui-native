using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Tree of expandable rows — rounded-md px-2 py-1.5 text-sm rows, hover:bg-muted, bg-accent when
// selected; children are indented behind a hairline. Clicking a row selects it and, when it has
// children, expands or collapses it. Rows are tab stops: Enter / Space act like a click, Right
// expands and Left collapses.
//   <ui:TreeView SelectedValue="{Binding Path}">
//       <ui:TreeViewItem Text="src" Icon="Folder" IsExpanded="True">
//           <ui:TreeViewItem Text="App.axaml" Icon="File" />
//       </ui:TreeViewItem>
//   </ui:TreeView>
public class TreeView : Border
{
    // The selected item's Value (its Text when no Value is set).
    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<TreeView, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    private readonly StackPanel _stack = new() { Spacing = 2 };

    static TreeView()
    {
        SelectedValueProperty.Changed.AddClassHandler<TreeView>((t, _) =>
        {
            t.Refresh();
            if (t._byUser) t.SelectedValueChanged?.Invoke(t, t.SelectedValue);
        });
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    [Content]
    public Controls Items => _stack.Children;

    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    // A click or key on a row.
    internal void SelectByUser(string? value) => ByUser(() => SelectedValue = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public TreeView()
    {
        Child = _stack;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh();
    }

    // Items don't track the selection: the tree pushes it to every item under it.
    private void Refresh()
    {
        foreach (var item in this.FindDescendantsOfType<TreeViewItem>())
            item.ApplySelection(SelectedValue);
    }
}

// One row of a TreeView. Nested TreeViewItems become its children.
public class TreeViewItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TreeViewItem, string?>(nameof(Text));

    // Identifies the item for selection. Defaults to Text.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<TreeViewItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<TreeViewItem, IconName>(nameof(Icon));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);

    private readonly Border _row;
    private readonly Icon _chevron;
    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly Panel _childrenHost;
    private readonly StackPanel _children = new() { Spacing = 2, Margin = new Thickness(25, 0, 0, 0) };
    private bool _selected;

    static TreeViewItem()
    {
        TextProperty.Changed.AddClassHandler<TreeViewItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<TreeViewItem>((i, _) => i.Update());
        IsExpandedProperty.Changed.AddClassHandler<TreeViewItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<TreeViewItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    [Content]
    public Controls Items => _children.Children;

    public string EffectiveValue => Value ?? Text ?? string.Empty;

    public TreeViewItem()
    {
        _chevron = new Icon { Kind = IconName.ChevronRight, Size = 16, Token = ShellToken.MutedForeground };
        _icon = new Icon { Size = 16, Token = ShellToken.MutedForeground, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(_icon, 1);
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Grid.SetColumn(_label, 2);

        // The chevron keeps its 16px slot on leaves, so labels line up within a level.
        var slot = new Panel { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center, Children = { _chevron } };
        _row = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"), ColumnSpacing = 6, Children = { slot, _icon, _label } },
            Height = 32,
            Padding = new Thickness(8, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        ShellFocus.Ring(_row, _row);
        _row.PointerEntered += (_, _) => Paint();
        _row.PointerExited += (_, _) => Paint();
        _row.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            Press();
            e.Handled = true;
        };
        _row.KeyDown += OnRowKeyDown;

        // ml-4 border-l pl-2: a hairline under the parent's chevron, children to its right.
        var guide = new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(16, 0, 0, 0) };
        guide.Token(BackgroundProperty, ShellToken.Border);
        _children.Children.CollectionChanged += (_, _) => Update();
        _childrenHost = new Panel { Margin = new Thickness(0, 2, 0, 0), IsVisible = false, Children = { guide, _children } };

        Child = new StackPanel { Children = { _row, _childrenHost } };
        Update();
    }

    private bool HasChildren => _children.Children.Count > 0;

    private void OnRowKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEffectivelyEnabled) return;
        if (e.Key is Key.Enter or Key.Space) Press();
        else if (e.Key == Key.Right && HasChildren && !IsExpanded) IsExpanded = true;
        else if (e.Key == Key.Left && HasChildren && IsExpanded) IsExpanded = false;
        else return;
        e.Handled = true;
    }

    private void Press()
    {
        if (!IsEffectivelyEnabled) return;
        if (HasChildren) IsExpanded = !IsExpanded;
        if (this.FindParentOfType<TreeView>() is { } tree) tree.SelectByUser(EffectiveValue);
    }

    internal void ApplySelection(string? selected)
    {
        _selected = selected != null && selected == EffectiveValue;
        Paint();
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _chevron.IsVisible = HasChildren;
        _chevron.Kind = IsExpanded ? IconName.ChevronDown : IconName.ChevronRight;
        _childrenHost.IsVisible = HasChildren && IsExpanded;
        Paint();
    }

    private void Paint()
    {
        _label.FontWeight = _selected ? FontWeight.Medium : FontWeight.Normal;
        _label.Token(TextBlock.ForegroundProperty, _selected ? ShellToken.AccentForeground : ShellToken.Foreground);
        if (_selected) _row.Token(BackgroundProperty, ShellToken.Accent);
        else if (_row.IsPointerOver && IsEffectivelyEnabled) _row.Token(BackgroundProperty, ShellToken.Muted);
        else
        {
            // Transparent, not null, so the whole row stays hit-testable.
            _row.ClearToken(BackgroundProperty);
            _row.Background = Brushes.Transparent;
        }
    }
}
