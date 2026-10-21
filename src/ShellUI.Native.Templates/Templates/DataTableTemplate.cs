using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DataTableTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "data-table",
        DisplayName = "Data Table",
        Description = "Table filled from a list, with filter, sorting, selection and pages",
        Category = ComponentCategory.DataDisplay,
        FilePath = "DataTable.cs",
        Dependencies = new List<string> { "shell", "icon", "button", "table", "input", "checkbox" },
        Tags = new List<string> { "data", "table", "grid", "sort", "filter", "pagination" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Data table — a Table filled from a list of objects, with a filter field, sortable column
// headers, optional row selection and pages.
//   <ui:DataTable ItemsSource=""{Binding Invoices}"" ShowSelection=""True"" PageSize=""5"">
//       <ui:DataTableColumn Header=""Invoice"" Property=""Number"" IsBold=""True"" />
//       <ui:DataTableColumn Header=""Status"" Property=""Status"" />
//       <ui:DataTableColumn Header=""Amount"" Property=""Amount"" Format=""C"" Width=""120"" HorizontalTextAlignment=""End"" />
//   </ui:DataTable>
// A column reads its Property from each item by name. With trimming or NativeAOT, set the
// column's Value in code instead: column.Value = item => ((Invoice)item).Number;
// For a view in a cell (a badge, a link), set Cell in code:
//   column.Cell = item => new Badge { Text = ((Invoice)item).Status };   (before ItemsSource)
[ContentProperty(nameof(Columns))]
public partial class DataTable : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(DataTable), null,
            propertyChanged: (b, o, n) => ((DataTable)b).OnItemsSourceChanged(o, n));

    public static readonly BindableProperty ShowFilterProperty =
        BindableProperty.Create(nameof(ShowFilter), typeof(bool), typeof(DataTable), true,
            propertyChanged: (b, o, n) => ((DataTable)b).Refresh());

    public static readonly BindableProperty FilterPlaceholderProperty =
        BindableProperty.Create(nameof(FilterPlaceholder), typeof(string), typeof(DataTable), ""Filter..."",
            propertyChanged: (b, o, n) => ((DataTable)b)._filter.Placeholder = n as string ?? string.Empty);

    public static readonly BindableProperty ShowPaginationProperty =
        BindableProperty.Create(nameof(ShowPagination), typeof(bool), typeof(DataTable), true,
            propertyChanged: (b, o, n) => ((DataTable)b).Refresh());

    // Rows per page while ShowPagination is on.
    public static readonly BindableProperty PageSizeProperty =
        BindableProperty.Create(nameof(PageSize), typeof(int), typeof(DataTable), 10,
            propertyChanged: (b, o, n) => ((DataTable)b).Refresh());

    // Adds a checkbox column; the header checkbox selects every row that passes the filter.
    public static readonly BindableProperty ShowSelectionProperty =
        BindableProperty.Create(nameof(ShowSelection), typeof(bool), typeof(DataTable), false,
            propertyChanged: (b, o, n) => ((DataTable)b).Refresh());

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(DataTable), ""No results."",
            propertyChanged: (b, o, n) => ((DataTable)b).Refresh());

    // Below this width the table scrolls sideways instead of squeezing its columns. 0 never scrolls.
    public static readonly BindableProperty MinimumContentWidthProperty =
        BindableProperty.Create(nameof(MinimumContentWidth), typeof(double), typeof(DataTable), 0d,
            propertyChanged: (b, o, n) => ((DataTable)b)._table.MinimumContentWidth = (double)n);

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool ShowFilter
    {
        get => (bool)GetValue(ShowFilterProperty);
        set => SetValue(ShowFilterProperty, value);
    }

    public string FilterPlaceholder
    {
        get => (string)GetValue(FilterPlaceholderProperty);
        set => SetValue(FilterPlaceholderProperty, value);
    }

    public bool ShowPagination
    {
        get => (bool)GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public bool ShowSelection
    {
        get => (bool)GetValue(ShowSelectionProperty);
        set => SetValue(ShowSelectionProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public double MinimumContentWidth
    {
        get => (double)GetValue(MinimumContentWidthProperty);
        set => SetValue(MinimumContentWidthProperty, value);
    }

    public IList<DataTableColumn> Columns => _columns;

    // The selected items, in the order of ItemsSource.
    public IReadOnlyList<object> SelectedItems => Source().Where(_selected.Contains).ToList();

    // The page shown, from 1.
    public int Page { get; private set; } = 1;

    public int PageCount { get; private set; } = 1;

    // The text in the filter field.
    public string Filter
    {
        get => _filter.Text ?? string.Empty;
        set => _filter.Text = value;
    }

    // User input only: ClearSelection and changes from code don't raise it.
    public event EventHandler<IReadOnlyList<object>>? SelectionChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    // Raised with the row's item.
    public event EventHandler<object>? RowTapped;

    private readonly ObservableCollection<DataTableColumn> _columns = new();
    private readonly HashSet<object> _selected = new();
    private readonly Input _filter;
    private readonly Table _table;
    private readonly Grid _footer;
    private readonly Label _summary;
    private readonly Label _pageLabel;
    private readonly Button _previous;
    private readonly Button _next;
    private readonly List<(object Item, TableRow Row, Checkbox? Check)> _rowViews = new();
    private Checkbox? _allCheck;
    private List<object> _view = new();
    private DataTableColumn? _sortColumn;
    private bool _descending;
    private bool _rendering;

    public DataTable()
    {
        _filter = new Input { Placeholder = FilterPlaceholder, MaximumWidthRequest = 384, HorizontalOptions = LayoutOptions.Start, MinimumWidthRequest = 240 };
        _filter.TextChanged += (_, _) => { Page = 1; Refresh(); };

        _table = new Table();

        _summary = new Label { FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation };
        _summary.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _pageLabel = new Label { FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 4, 0) };
        _pageLabel.Token(Label.TextColorProperty, ShellToken.Foreground);
        _previous = new Button { Text = ""Previous"", Variant = ButtonVariant.Outline, Size = ButtonSize.Sm };
        _previous.Clicked += (_, _) => GoToPage(Page - 1);
        _next = new Button { Text = ""Next"", Variant = ButtonVariant.Outline, Size = ButtonSize.Sm };
        _next.Clicked += (_, _) => GoToPage(Page + 1);

        var pager = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { _pageLabel, _previous, _next } };
        _footer = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        _footer.Add(_summary, 0, 0);
        _footer.Add(pager, 1, 0);

        Content = new VerticalStackLayout { Spacing = 12, Children = { _filter, _table, _footer } };
        _columns.CollectionChanged += (_, _) => Refresh();
        Refresh();
    }

    // Re-reads the items and columns. Call it after changing an item's values or a column.
    public void Refresh()
    {
        // The table and its parts don't exist yet while the constructor sets property defaults.
        if (_footer is null) return;
        var items = Source();
        _selected.IntersectWith(items);

        var words = Filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        IEnumerable<object> view = ShowFilter && words.Length > 0
            ? items.Where(item => words.All(word => _columns.Any(c => c.Text(item).Contains(word, StringComparison.CurrentCultureIgnoreCase))))
            : items;
        if (_sortColumn != null && _columns.Contains(_sortColumn))
        {
            var column = _sortColumn;
            view = _descending
                ? view.OrderByDescending(item => column.Read(item), ValueComparer.Instance)
                : view.OrderBy(item => column.Read(item), ValueComparer.Instance);
        }
        _view = view.ToList();

        var size = Math.Max(1, PageSize);
        PageCount = ShowPagination ? Math.Max(1, (int)Math.Ceiling(_view.Count / (double)size)) : 1;
        Page = Math.Clamp(Page, 1, PageCount);
        Render(ShowPagination ? _view.Skip((Page - 1) * size).Take(size).ToList() : _view);
    }

    public void GoToPage(int page)
    {
        Page = page;
        Refresh();
    }

    // Sorts by a column; call it again to reverse, and with null to restore the source order.
    public void SortBy(DataTableColumn? column, bool descending = false)
    {
        _sortColumn = column;
        _descending = descending;
        Refresh();
    }

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void ClearSelection()
    {
        if (_selected.Count == 0) return;
        _selected.Clear();
        OnSelectionChanged();
    }

    private List<object> Source() => ItemsSource?.Cast<object>().Where(item => item != null).ToList() ?? new List<object>();

    private void OnItemsSourceChanged(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnItemsChanged;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnItemsChanged;
        Page = 1;
        Refresh();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void Render(IReadOnlyList<object> rows)
    {
        _rendering = true;
        _filter.IsVisible = ShowFilter;
        var widths = _columns.Select(c => string.IsNullOrWhiteSpace(c.Width) ? ""*"" : c.Width);
        _table.Columns = string.Join("","", ShowSelection ? widths.Prepend(""48"") : widths);
        _table.Rows.Clear();
        _rowViews.Clear();
        _allCheck = null;

        var header = new TableHeader();
        if (ShowSelection)
        {
            var all = new Checkbox { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            SemanticProperties.SetDescription(all, ""Select all rows"");
            all.CheckedChanged += (_, isChecked) =>
            {
                if (_rendering) return;
                if (isChecked) _selected.UnionWith(_view);
                else _selected.ExceptWith(_view);
                ByUser(OnSelectionChanged);
            };
            header.Children.Add(all);
            _allCheck = all;
        }
        foreach (var column in _columns) header.Children.Add(CreateHead(column));
        _table.Rows.Add(header);

        foreach (var item in rows)
        {
            var row = new TableRow();
            row.Tapped += (_, _) => RowTapped?.Invoke(this, item);
            Checkbox? check = null;
            if (ShowSelection)
            {
                check = new Checkbox { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                SemanticProperties.SetDescription(check, ""Select row"");
                check.CheckedChanged += (_, isChecked) =>
                {
                    if (_rendering) return;
                    if (isChecked) _selected.Add(item);
                    else _selected.Remove(item);
                    ByUser(OnSelectionChanged);
                };
                row.Children.Add(check);
            }
            _rowViews.Add((item, row, check));
            foreach (var column in _columns)
                row.Children.Add(column.Cell is { } cell
                    ? new TableCell { Content = cell(item) }
                    : new TableCell { Text = column.Text(item), IsBold = column.IsBold, HorizontalTextAlignment = column.HorizontalTextAlignment });
            _table.Rows.Add(row);
        }

        if (rows.Count == 0)
        {
            // h-24 text-center, one cell across the table.
            var empty = new TableHeader { Columns = ""*"" };
            empty.Children.Add(new TableCell { Text = EmptyText ?? string.Empty, HorizontalTextAlignment = TextAlignment.Center, HeightRequest = 96 });
            _table.Rows.Add(empty);
        }

        _footer.IsVisible = ShowPagination || ShowSelection;
        ShowSelectionState();
        _pageLabel.Text = $""Page {Page} of {PageCount}"";
        _pageLabel.IsVisible = _previous.IsVisible = _next.IsVisible = ShowPagination;
        _previous.IsEnabled = Page > 1;
        _next.IsEnabled = Page < PageCount;
        _rendering = false;
    }

    // Ticks, row tints and the count follow the selection without rebuilding the rows.
    private void ShowSelectionState()
    {
        var rendering = _rendering;
        _rendering = true;
        foreach (var (item, row, check) in _rowViews)
        {
            var selected = _selected.Contains(item);
            row.IsSelected = selected;
            if (check != null) check.IsChecked = selected;
        }
        if (_allCheck != null) _allCheck.IsChecked = _view.Count > 0 && _view.All(_selected.Contains);
        _summary.Text = ShowSelection
            ? $""{_view.Count(_selected.Contains)} of {_view.Count} row(s) selected.""
            : $""{_view.Count} row(s)."";
        _rendering = rendering;
    }

    private void OnSelectionChanged()
    {
        ShowSelectionState();
        if (_byUser) SelectionChanged?.Invoke(this, SelectedItems);
    }

    // h-11 px-4 font-medium text-muted-foreground; a sortable header also shows its direction
    // and cycles ascending, descending, unsorted on tap.
    private View CreateHead(DataTableColumn column)
    {
        var label = new Label
        {
            Text = column.Header ?? string.Empty,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        label.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        var row = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = column.HorizontalTextAlignment switch
            {
                TextAlignment.End => LayoutOptions.End,
                TextAlignment.Center => LayoutOptions.Center,
                _ => LayoutOptions.Start
            },
            Children = { label }
        };
        var head = new ContentView { Content = row, HeightRequest = 44, Padding = new Thickness(16, 0) };
        if (!column.IsSortable) return head;

        var sorted = ReferenceEquals(_sortColumn, column);
        row.Children.Add(new Icon
        {
            Name = !sorted ? IconName.ChevronsUpDown : _descending ? IconName.ArrowDown : IconName.ArrowUp,
            Size = 14,
            Token = sorted ? ShellToken.Foreground : ShellToken.MutedForeground,
            VerticalOptions = LayoutOptions.Center
        });
        if (sorted) label.Token(Label.TextColorProperty, ShellToken.Foreground);
        SemanticProperties.SetDescription(head, $""Sort by {column.Header}"");
        // A layout with no background only takes pointer input over its children.
        head.BackgroundColor = Colors.Transparent;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Dispatcher.Dispatch(() => ToggleSort(column));
        head.GestureRecognizers.Add(tap);
        return head;
    }

    private void ToggleSort(DataTableColumn column)
    {
        if (!ReferenceEquals(_sortColumn, column)) SortBy(column);
        else if (!_descending) SortBy(column, descending: true);
        else SortBy(null);
    }

    // Numbers and dates sort by value, everything else by its text; nulls go first.
    private sealed class ValueComparer : IComparer<object?>
    {
        public static readonly ValueComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (x is null || y is null) return x is null ? (y is null ? 0 : -1) : 1;
            if (x is string || y is string || x.GetType() != y.GetType() || x is not IComparable comparable)
                return string.Compare(Convert.ToString(x, CultureInfo.CurrentCulture), Convert.ToString(y, CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase);
            return comparable.CompareTo(y);
        }
    }
}

// One column of a DataTable.
public partial class DataTableColumn
{
    public string? Header { get; set; }

    // The name of the item property this column shows.
    public string? Property { get; set; }

    // Reads the column's value from an item; replaces Property.
    public Func<object, object?>? Value { get; set; }

    // Draws the cell as any view (a link, a badge, an amount); replaces the text. Filter and sort
    // still use Value or Property, so set one of them for a column that sorts or is searched. Set it
    // before ItemsSource, or call Refresh() after.
    public Func<object, View>? Cell { get; set; }

    // A .NET format string for the value, e.g. ""C"" or ""MMM d, yyyy"".
    public string? Format { get; set; }

    // A Grid column width: ""*"", ""2*"" or a fixed number. Auto is not supported.
    public string Width { get; set; } = ""*"";

    public bool IsSortable { get; set; } = true;

    public bool IsBold { get; set; }

    public TextAlignment HorizontalTextAlignment { get; set; } = TextAlignment.Start;

    [UnconditionalSuppressMessage(""Trimming"", ""IL2075"", Justification = ""Property is looked up by name on the item's runtime type; set Value instead when trimming."")]
    internal object? Read(object item)
    {
        if (Value != null) return Value(item);
        return string.IsNullOrEmpty(Property) ? null : item.GetType().GetProperty(Property)?.GetValue(item);
    }

    internal string Text(object item)
    {
        var value = Read(item);
        if (value is null) return string.Empty;
        return !string.IsNullOrEmpty(Format) && value is IFormattable formattable
            ? formattable.ToString(Format, CultureInfo.CurrentCulture)
            : Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Data table — a Table filled from a list of objects, with a filter field, sortable column
// headers, optional row selection and pages. Sortable headers are tab stops (Enter / Space).
//   <ui:DataTable ItemsSource=""{Binding Invoices}"" ShowSelection=""True"" PageSize=""5"">
//       <ui:DataTableColumn Header=""Invoice"" Property=""Number"" IsBold=""True"" />
//       <ui:DataTableColumn Header=""Status"" Property=""Status"" />
//       <ui:DataTableColumn Header=""Amount"" Property=""Amount"" Format=""C"" Width=""120"" HorizontalTextAlignment=""End"" />
//   </ui:DataTable>
// A column reads its Property from each item by name. With trimming or NativeAOT, set the
// column's Value in code instead: column.Value = item => ((Invoice)item).Number;
// For a view in a cell (a badge, a link), set Cell in code:
//   column.Cell = item => new Badge { Text = ((Invoice)item).Status };   (before ItemsSource)
public class DataTable : Border
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DataTable, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<bool> ShowFilterProperty =
        AvaloniaProperty.Register<DataTable, bool>(nameof(ShowFilter), true);

    public static readonly StyledProperty<string?> FilterPlaceholderProperty =
        AvaloniaProperty.Register<DataTable, string?>(nameof(FilterPlaceholder), ""Filter..."");

    public static readonly StyledProperty<bool> ShowPaginationProperty =
        AvaloniaProperty.Register<DataTable, bool>(nameof(ShowPagination), true);

    // Rows per page while ShowPagination is on.
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<DataTable, int>(nameof(PageSize), 10);

    // Adds a checkbox column; the header checkbox selects every row that passes the filter.
    public static readonly StyledProperty<bool> ShowSelectionProperty =
        AvaloniaProperty.Register<DataTable, bool>(nameof(ShowSelection));

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<DataTable, string?>(nameof(EmptyText), ""No results."");

    // Below this width the table scrolls sideways instead of squeezing its columns. 0 never scrolls.
    public static readonly StyledProperty<double> MinimumContentWidthProperty =
        AvaloniaProperty.Register<DataTable, double>(nameof(MinimumContentWidth));

    private readonly HashSet<object> _selected = new();
    private readonly List<(object Item, TableRow Row, Checkbox? Check)> _rowViews = new();
    private readonly Input _filter;
    private readonly Table _table = new();
    private readonly Grid _footer;
    private readonly TextBlock _summary;
    private readonly TextBlock _pageLabel;
    private readonly Button _previous;
    private readonly Button _next;
    private Checkbox? _allCheck;
    private List<object> _view = new();
    private DataTableColumn? _sortColumn;
    private DataTableColumn? _refocus;
    private bool _descending;
    private bool _rendering;

    static DataTable()
    {
        ItemsSourceProperty.Changed.AddClassHandler<DataTable>((t, e) => t.OnItemsSourceChanged(e.OldValue, e.NewValue));
        ShowFilterProperty.Changed.AddClassHandler<DataTable>((t, _) => t.Refresh());
        FilterPlaceholderProperty.Changed.AddClassHandler<DataTable>((t, e) => t._filter.Placeholder = (string?)e.NewValue);
        ShowPaginationProperty.Changed.AddClassHandler<DataTable>((t, _) => t.Refresh());
        PageSizeProperty.Changed.AddClassHandler<DataTable>((t, _) => t.Refresh());
        ShowSelectionProperty.Changed.AddClassHandler<DataTable>((t, _) => t.Refresh());
        EmptyTextProperty.Changed.AddClassHandler<DataTable>((t, _) => t.Refresh());
        MinimumContentWidthProperty.Changed.AddClassHandler<DataTable>((t, e) => t._table.MinimumContentWidth = (double)e.NewValue!);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool ShowFilter
    {
        get => GetValue(ShowFilterProperty);
        set => SetValue(ShowFilterProperty, value);
    }

    public string? FilterPlaceholder
    {
        get => GetValue(FilterPlaceholderProperty);
        set => SetValue(FilterPlaceholderProperty, value);
    }

    public bool ShowPagination
    {
        get => GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public bool ShowSelection
    {
        get => GetValue(ShowSelectionProperty);
        set => SetValue(ShowSelectionProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public double MinimumContentWidth
    {
        get => GetValue(MinimumContentWidthProperty);
        set => SetValue(MinimumContentWidthProperty, value);
    }

    [Content]
    public AvaloniaList<DataTableColumn> Columns { get; } = new();

    // The selected items, in the order of ItemsSource.
    public IReadOnlyList<object> SelectedItems => Source().Where(_selected.Contains).ToList();

    // The page shown, from 1.
    public int Page { get; private set; } = 1;

    public int PageCount { get; private set; } = 1;

    // The text in the filter field.
    public string Filter
    {
        get => _filter.Text ?? string.Empty;
        set => _filter.Text = value;
    }

    // User input only: ClearSelection and changes from code don't raise it.
    public event EventHandler<IReadOnlyList<object>>? SelectionChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    // Raised with the row's item.
    public event EventHandler<object>? RowTapped;

    public DataTable()
    {
        _filter = new Input { Placeholder = FilterPlaceholder, MaxWidth = 384, MinWidth = 240, HorizontalAlignment = HorizontalAlignment.Left };
        _filter.TextChanged += (_, _) => { Page = 1; Refresh(); };

        _summary = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _summary.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _pageLabel = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) };
        _pageLabel.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _previous = new Button { Text = ""Previous"", Variant = ButtonVariant.Outline, Size = ButtonSize.Sm };
        _previous.Clicked += (_, _) => GoToPage(Page - 1);
        _next = new Button { Text = ""Next"", Variant = ButtonVariant.Outline, Size = ButtonSize.Sm };
        _next.Clicked += (_, _) => GoToPage(Page + 1);

        var pager = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center, Children = { _pageLabel, _previous, _next } };
        Grid.SetColumn(pager, 1);
        _footer = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto""), ColumnSpacing = 12, Children = { _summary, pager } };

        Child = new StackPanel { Spacing = 12, Children = { _filter, _table, _footer } };
        Columns.CollectionChanged += (_, _) => Refresh();
        Refresh();
    }

    // Re-reads the items and columns. Call it after changing an item's values or a column.
    public void Refresh()
    {
        // The table and its parts don't exist yet while property defaults are applied.
        if (_footer is null) return;
        var items = Source();
        _selected.IntersectWith(items);

        var words = Filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        IEnumerable<object> view = ShowFilter && words.Length > 0
            ? items.Where(item => words.All(word => Columns.Any(c => c.Text(item).Contains(word, StringComparison.CurrentCultureIgnoreCase))))
            : items;
        if (_sortColumn != null && Columns.Contains(_sortColumn))
        {
            var column = _sortColumn;
            view = _descending
                ? view.OrderByDescending(item => column.Read(item), ValueComparer.Instance)
                : view.OrderBy(item => column.Read(item), ValueComparer.Instance);
        }
        _view = view.ToList();

        var size = Math.Max(1, PageSize);
        PageCount = ShowPagination ? Math.Max(1, (int)Math.Ceiling(_view.Count / (double)size)) : 1;
        Page = Math.Clamp(Page, 1, PageCount);
        Render(ShowPagination ? _view.Skip((Page - 1) * size).Take(size).ToList() : _view);
    }

    public void GoToPage(int page)
    {
        Page = page;
        Refresh();
    }

    // Sorts by a column; call it again to reverse, and with null to restore the source order.
    public void SortBy(DataTableColumn? column, bool descending = false)
    {
        _sortColumn = column;
        _descending = descending;
        Refresh();
    }

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void ClearSelection()
    {
        if (_selected.Count == 0) return;
        _selected.Clear();
        OnSelectionChanged();
    }

    private List<object> Source() => ItemsSource?.Cast<object>().Where(item => item != null).ToList() ?? new List<object>();

    private void OnItemsSourceChanged(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnItemsChanged;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnItemsChanged;
        Page = 1;
        Refresh();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void Render(IReadOnlyList<object> rows)
    {
        _rendering = true;
        _filter.IsVisible = ShowFilter;
        var widths = Columns.Select(c => string.IsNullOrWhiteSpace(c.Width) ? ""*"" : c.Width);
        _table.Columns = string.Join("","", ShowSelection ? widths.Prepend(""48"") : widths);
        _table.Rows.Clear();
        _rowViews.Clear();
        _allCheck = null;

        var header = new TableHeader();
        if (ShowSelection)
        {
            var all = new Checkbox { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            AutomationProperties.SetName(all, ""Select all rows"");
            all.CheckedChanged += (_, isChecked) =>
            {
                if (_rendering) return;
                if (isChecked) _selected.UnionWith(_view);
                else _selected.ExceptWith(_view);
                ByUser(OnSelectionChanged);
            };
            header.Children.Add(all);
            _allCheck = all;
        }
        foreach (var column in Columns) header.Children.Add(CreateHead(column));
        _table.Rows.Add(header);

        foreach (var item in rows)
        {
            var row = new TableRow();
            // A click on the row's checkbox selects; it is not a click on the row.
            row.Tapped += (_, e) =>
            {
                if (e.Source is Visual source && source.FindAncestorOfType<Checkbox>(includeSelf: true) != null) return;
                RowTapped?.Invoke(this, item);
            };
            Checkbox? check = null;
            if (ShowSelection)
            {
                check = new Checkbox { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                AutomationProperties.SetName(check, ""Select row"");
                check.CheckedChanged += (_, isChecked) =>
                {
                    if (_rendering) return;
                    if (isChecked) _selected.Add(item);
                    else _selected.Remove(item);
                    ByUser(OnSelectionChanged);
                };
                row.Children.Add(check);
            }
            _rowViews.Add((item, row, check));
            foreach (var column in Columns)
                row.Children.Add(column.Cell is { } cell
                    ? new TableCell { Child = cell(item) }
                    : new TableCell { Text = column.Text(item), IsBold = column.IsBold, HorizontalTextAlignment = column.HorizontalTextAlignment });
            _table.Rows.Add(row);
        }

        if (rows.Count == 0)
        {
            // h-24 text-center, one cell across the table.
            var empty = new TableHeader { Columns = ""*"" };
            empty.Children.Add(new TableCell { Text = EmptyText ?? string.Empty, HorizontalTextAlignment = TextAlignment.Center, Height = 96 });
            _table.Rows.Add(empty);
        }

        _footer.IsVisible = ShowPagination || ShowSelection;
        ShowSelectionState();
        _pageLabel.Text = $""Page {Page} of {PageCount}"";
        _pageLabel.IsVisible = _previous.IsVisible = _next.IsVisible = ShowPagination;
        _previous.IsEnabled = Page > 1;
        _next.IsEnabled = Page < PageCount;
        _rendering = false;
    }

    // Ticks, row tints and the count follow the selection without rebuilding the rows.
    private void ShowSelectionState()
    {
        var rendering = _rendering;
        _rendering = true;
        foreach (var (item, row, check) in _rowViews)
        {
            var selected = _selected.Contains(item);
            row.IsSelected = selected;
            if (check != null) check.IsChecked = selected;
        }
        if (_allCheck != null) _allCheck.IsChecked = _view.Count > 0 && _view.All(_selected.Contains);
        _summary.Text = ShowSelection
            ? $""{_view.Count(_selected.Contains)} of {_view.Count} row(s) selected.""
            : $""{_view.Count} row(s)."";
        _rendering = rendering;
    }

    private void OnSelectionChanged()
    {
        ShowSelectionState();
        if (_byUser) SelectionChanged?.Invoke(this, SelectedItems);
    }

    // h-11 px-4 font-medium text-muted-foreground; a sortable header also shows its direction
    // and cycles ascending, descending, unsorted on click.
    private Control CreateHead(DataTableColumn column)
    {
        var label = new TextBlock
        {
            Text = column.Header ?? string.Empty,
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = column.HorizontalTextAlignment switch
            {
                TextAlignment.End or TextAlignment.Right => HorizontalAlignment.Right,
                TextAlignment.Center => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Left
            },
            Children = { label }
        };
        var head = new Border { Child = row, Height = 44, Padding = new Thickness(16, 0) };
        if (!column.IsSortable) return head;

        var sorted = ReferenceEquals(_sortColumn, column);
        row.Children.Add(new Icon
        {
            Kind = !sorted ? IconName.ChevronsUpDown : _descending ? IconName.ArrowDown : IconName.ArrowUp,
            Size = 14,
            Token = sorted ? ShellToken.Foreground : ShellToken.MutedForeground,
            VerticalAlignment = VerticalAlignment.Center
        });
        if (sorted) label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        AutomationProperties.SetName(head, $""Sort by {column.Header}"");
        // Transparent, not null, so the whole header cell takes the pointer.
        head.Background = Brushes.Transparent;
        head.Cursor = new Cursor(StandardCursorType.Hand);
        head.CornerRadius = new CornerRadius(ShellTheme.RadiusSm);
        ShellFocus.Ring(head, head);
        // Sorting rebuilds the header row, so the rebuild waits until this event is done.
        head.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            Dispatcher.UIThread.Post(() => ToggleSort(column));
            e.Handled = true;
        };
        head.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            // Keep keyboard focus on this column's new header cell.
            _refocus = column;
            Dispatcher.UIThread.Post(() => ToggleSort(column));
            e.Handled = true;
        };
        if (ReferenceEquals(_refocus, column))
        {
            _refocus = null;
            Dispatcher.UIThread.Post(() => head.Focus(NavigationMethod.Tab), DispatcherPriority.Loaded);
        }
        return head;
    }

    private void ToggleSort(DataTableColumn column)
    {
        if (!ReferenceEquals(_sortColumn, column)) SortBy(column);
        else if (!_descending) SortBy(column, descending: true);
        else SortBy(null);
    }

    // Numbers and dates sort by value, everything else by its text; nulls go first.
    private sealed class ValueComparer : IComparer<object?>
    {
        public static readonly ValueComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (x is null || y is null) return x is null ? (y is null ? 0 : -1) : 1;
            if (x is string || y is string || x.GetType() != y.GetType() || x is not IComparable comparable)
                return string.Compare(Convert.ToString(x, CultureInfo.CurrentCulture), Convert.ToString(y, CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase);
            return comparable.CompareTo(y);
        }
    }
}

// One column of a DataTable.
public class DataTableColumn
{
    public string? Header { get; set; }

    // The name of the item property this column shows.
    public string? Property { get; set; }

    // Reads the column's value from an item; replaces Property.
    public Func<object, object?>? Value { get; set; }

    // Draws the cell as any control (a link, a badge, an amount); replaces the text. Filter and sort
    // still use Value or Property, so set one of them for a column that sorts or is searched. Set it
    // before ItemsSource, or call Refresh() after.
    public Func<object, Control>? Cell { get; set; }

    // A .NET format string for the value, e.g. ""C"" or ""MMM d, yyyy"".
    public string? Format { get; set; }

    // A Grid column width: ""*"", ""2*"" or a fixed number. Auto is not supported.
    public string Width { get; set; } = ""*"";

    public bool IsSortable { get; set; } = true;

    public bool IsBold { get; set; }

    public TextAlignment HorizontalTextAlignment { get; set; } = TextAlignment.Start;

    [UnconditionalSuppressMessage(""Trimming"", ""IL2075"", Justification = ""Property is looked up by name on the item's runtime type; set Value instead when trimming."")]
    internal object? Read(object item)
    {
        if (Value != null) return Value(item);
        return string.IsNullOrEmpty(Property) ? null : item.GetType().GetProperty(Property)?.GetValue(item);
    }

    internal string Text(object item)
    {
        var value = Read(item);
        if (value is null) return string.Empty;
        return !string.IsNullOrEmpty(Format) && value is IFormattable formattable
            ? formattable.ToString(Format, CultureInfo.CurrentCulture)
            : Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
"
    };
}
