using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PaginationTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "pagination",
        DisplayName = "Pagination",
        Description = "Page navigation with previous/next and ellipsis",
        Category = ComponentCategory.Navigation,
        FilePath = "Pagination.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "pagination", "pages", "navigation" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Page navigation — Previous / 1 … 4 [5] 6 … 20 / Next. Page buttons are 36px ghost buttons;
// the current page is outlined. Collapses long ranges into ellipses around the current page.
// Usage: <ui:Pagination Page=""{Binding Page}"" TotalPages=""20"" PageChanged=""OnPage"" />
public partial class Pagination : ContentView
{
    public static readonly BindableProperty PageProperty =
        BindableProperty.Create(nameof(Page), typeof(int), typeof(Pagination), 1, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Pagination)b).OnPageChanged());

    public static readonly BindableProperty TotalPagesProperty =
        BindableProperty.Create(nameof(TotalPages), typeof(int), typeof(Pagination), 1,
            propertyChanged: (b, o, n) => ((Pagination)b).Rebuild());

    // Pages shown on each side of the current page.
    public static readonly BindableProperty SiblingCountProperty =
        BindableProperty.Create(nameof(SiblingCount), typeof(int), typeof(Pagination), 1,
            propertyChanged: (b, o, n) => ((Pagination)b).Rebuild());

    // ""Previous"" / ""Next"" text next to the chevrons. Off by default on phones to fit the width.
    public static readonly BindableProperty ShowLabelsProperty =
        BindableProperty.Create(nameof(ShowLabels), typeof(bool), typeof(Pagination),
            DeviceInfo.Idiom != DeviceIdiom.Phone, propertyChanged: (b, o, n) => ((Pagination)b).Rebuild());

    public int Page
    {
        get => (int)GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int SiblingCount
    {
        get => (int)GetValue(SiblingCountProperty);
        set => SetValue(SiblingCountProperty, value);
    }

    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    public event EventHandler<int>? PageChanged;

    private const double ItemSize = 36;

    private readonly HorizontalStackLayout _row;

    public Pagination()
    {
        _row = new HorizontalStackLayout { Spacing = 4 };
        Content = _row;
        HorizontalOptions = LayoutOptions.Start;
        Rebuild();
    }

    private void OnPageChanged()
    {
        Rebuild();
        PageChanged?.Invoke(this, Page);
    }

    // Page is not clamped when set (XAML may set Page before TotalPages), only when shown.
    private int Total => Math.Max(1, TotalPages);
    private int Current => Math.Clamp(Page, 1, Total);

    // First, last, and the current page with its siblings; 0 marks an ellipsis.
    private List<int> VisiblePages()
    {
        var total = Total;
        var siblings = Math.Max(0, SiblingCount);
        var pages = new List<int>();
        var from = Math.Max(2, Current - siblings);
        var to = Math.Min(total - 1, Current + siblings);

        pages.Add(1);
        if (from > 2) pages.Add(from == 3 ? 2 : 0);
        for (var p = from; p <= to; p++) pages.Add(p);
        if (to < total - 1) pages.Add(to == total - 2 ? total - 1 : 0);
        if (total > 1) pages.Add(total);
        return pages;
    }

    private void Rebuild()
    {
        _row.Children.Clear();
        _row.Children.Add(NavItem(IconName.ChevronLeft, ""Previous"", iconFirst: true, enabled: Current > 1, () => Page = Current - 1));
        foreach (var page in VisiblePages())
            _row.Children.Add(page == 0 ? Ellipsis() : PageItem(page));
        _row.Children.Add(NavItem(IconName.ChevronRight, ""Next"", iconFirst: false, enabled: Current < Total, () => Page = Current + 1));
    }

    private View PageItem(int page)
    {
        var current = page == Current;
        var label = new Label
        {
            Text = page.ToString(),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        label.Token(Label.TextColorProperty, ShellToken.Foreground);
        var item = Item(label, ItemSize, new Thickness(0), () => Page = page);
        item.MinimumWidthRequest = ItemSize;
        if (current)
        {
            // Outline variant for the current page; no hover change.
            item.StrokeThickness = 1;
            item.Token(Border.StrokeProperty, ShellToken.Input);
            item.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
            item.GestureRecognizers.Clear();
        }
        SemanticProperties.SetDescription(item, current ? $""Page {page}, current page"" : $""Go to page {page}"");
        return item;
    }

    private View NavItem(IconName icon, string text, bool iconFirst, bool enabled, Action go)
    {
        var chevron = new Icon { Name = icon, Size = 16 };
        var row = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        if (ShowLabels)
        {
            var label = new Label { Text = text, FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center };
            label.Token(Label.TextColorProperty, ShellToken.Foreground);
            if (iconFirst) { row.Children.Add(chevron); row.Children.Add(label); }
            else { row.Children.Add(label); row.Children.Add(chevron); }
        }
        else row.Children.Add(chevron);

        var item = Item(row, -1, new Thickness(ShowLabels ? 10 : 0, 0), go);
        item.MinimumWidthRequest = ItemSize;
        SemanticProperties.SetDescription(item, $""Go to {text.ToLowerInvariant()} page"");
        if (!enabled)
        {
            item.Opacity = 0.5;
            item.GestureRecognizers.Clear();
        }
        return item;
    }

    private static View Ellipsis() => new Grid
    {
        WidthRequest = ItemSize,
        HeightRequest = ItemSize,
        Children = { new Icon { Name = IconName.Ellipsis, Size = 16, Token = ShellToken.MutedForeground } }
    };

    // Ghost button: transparent, hover:bg-accent.
    private static Border Item(View content, double width, Thickness padding, Action onTap)
    {
        var item = new Border
        {
            Content = content,
            WidthRequest = width,
            HeightRequest = ItemSize,
            Padding = padding,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => item.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { item.ClearValue(VisualElement.BackgroundColorProperty); item.BackgroundColor = Colors.Transparent; };
        item.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onTap();
        item.GestureRecognizers.Add(tap);
        return item;
    }
}
"
    };
}
