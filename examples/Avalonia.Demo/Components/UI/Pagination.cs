using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Page navigation — Previous / 1 … 4 [5] 6 … 20 / Next. Page buttons are 36px ghost buttons;
// the current page is outlined. Collapses long ranges into ellipses around the current page.
// Every item is a tab stop that activates with Enter or Space.
// Usage: <ui:Pagination Page="{Binding Page}" TotalPages="20" PageChanged="OnPage" />
public class Pagination : Border
{
    public static readonly StyledProperty<int> PageProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(Page), 1, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> TotalPagesProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(TotalPages), 1);

    // Pages shown on each side of the current page.
    public static readonly StyledProperty<int> SiblingCountProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(SiblingCount), 1);

    // "Previous" / "Next" text next to the chevrons.
    public static readonly StyledProperty<bool> ShowLabelsProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(ShowLabels), true);

    private const double ItemSize = 36;

    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4 };
    private Control? _currentItem;

    static Pagination()
    {
        PageProperty.Changed.AddClassHandler<Pagination>((p, _) =>
        {
            p.Rebuild();
            p.PageChanged?.Invoke(p, p.Page);
        });
        TotalPagesProperty.Changed.AddClassHandler<Pagination>((p, _) => p.Rebuild());
        SiblingCountProperty.Changed.AddClassHandler<Pagination>((p, _) => p.Rebuild());
        ShowLabelsProperty.Changed.AddClassHandler<Pagination>((p, _) => p.Rebuild());
    }

    public int Page
    {
        get => GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    public int TotalPages
    {
        get => GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int SiblingCount
    {
        get => GetValue(SiblingCountProperty);
        set => SetValue(SiblingCountProperty, value);
    }

    public bool ShowLabels
    {
        get => GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    public event EventHandler<int>? PageChanged;

    public Pagination()
    {
        Child = _row;
        HorizontalAlignment = HorizontalAlignment.Left;
        Rebuild();
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
        // The rebuild replaces the focused item, so keyboard focus moves to the new current page.
        var hadFocus = _row.IsKeyboardFocusWithin;
        _row.Children.Clear();
        _row.Children.Add(NavItem(IconName.ChevronLeft, "Previous", iconFirst: true, enabled: Current > 1, () => Page = Current - 1));
        foreach (var page in VisiblePages())
            _row.Children.Add(page == 0 ? Ellipsis() : PageItem(page));
        _row.Children.Add(NavItem(IconName.ChevronRight, "Next", iconFirst: false, enabled: Current < Total, () => Page = Current + 1));
        if (hadFocus) _currentItem?.Focus(NavigationMethod.Tab);
    }

    private Control PageItem(int page)
    {
        var current = page == Current;
        var label = new TextBlock { Text = page.ToString(CultureInfo.CurrentCulture), FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        var item = Item(label, ItemSize, new Thickness(0), current ? null : () => Page = page, focusable: true);
        AutomationProperties.SetName(item, current ? $"Page {page}, current page" : $"Go to page {page}");
        if (current)
        {
            // Outline variant for the current page; no hover change.
            item.BorderThickness = new Thickness(1);
            item.Token(BorderBrushProperty, ShellToken.Input).Token(BackgroundProperty, ShellToken.Background);
            _currentItem = item;
        }
        return item;
    }

    private Control NavItem(IconName icon, string text, bool iconFirst, bool enabled, Action go)
    {
        var chevron = new Icon { Kind = icon, Size = 16, VerticalAlignment = VerticalAlignment.Center };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        if (ShowLabels)
        {
            var label = new TextBlock { Text = text, FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
            label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
            if (iconFirst) { row.Children.Add(chevron); row.Children.Add(label); }
            else { row.Children.Add(label); row.Children.Add(chevron); }
        }
        else row.Children.Add(chevron);

        var item = Item(row, double.NaN, new Thickness(ShowLabels ? 10 : 0, 0), enabled ? go : null, focusable: enabled);
        AutomationProperties.SetName(item, $"Go to {text.ToLowerInvariant()} page");
        if (!enabled) item.Opacity = 0.5;
        return item;
    }

    private static Control Ellipsis() => new Border
    {
        Width = ItemSize,
        Height = ItemSize,
        Child = new Icon { Kind = IconName.Ellipsis, Size = 16, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
    };

    // Ghost button: transparent, hover:bg-accent. With no action it doesn't react (the current
    // page, or Previous / Next at the ends); the current page is still a tab stop.
    private static Border Item(Control content, double width, Thickness padding, Action? onPress, bool focusable)
    {
        var item = new Border
        {
            Child = content,
            Width = width,
            MinWidth = ItemSize,
            Height = ItemSize,
            Padding = padding,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent
        };
        if (focusable) ShellFocus.Ring(item, item);
        if (onPress is null) return item;

        item.Cursor = new Cursor(StandardCursorType.Hand);
        item.PointerEntered += (_, _) => item.Token(BackgroundProperty, ShellToken.Accent);
        item.PointerExited += (_, _) => { item.ClearToken(BackgroundProperty); item.Background = Brushes.Transparent; };
        item.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            onPress();
            e.Handled = true;
        };
        item.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            onPress();
            e.Handled = true;
        };
        return item;
    }
}
