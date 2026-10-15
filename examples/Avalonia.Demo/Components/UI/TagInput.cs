using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Tag field — min-h-10 rounded-md border holding chips (rounded-md bg-secondary text-xs) and a
// text field that wraps with them. Enter, a comma or a semicolon turns what was typed into a
// tag; the × on a chip removes it, and Backspace in the empty field removes the last one.
// Usage: <ui:TagInput Tags="{Binding Labels}" Placeholder="Add label..." MaxTags="5" />
public class TagInput : Border
{
    public static readonly StyledProperty<IList<string>?> TagsProperty =
        AvaloniaProperty.Register<TagInput, IList<string>?>(nameof(Tags), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<TagInput, string?>(nameof(Placeholder), "Add tag...");

    // 0 means no limit.
    public static readonly StyledProperty<int> MaxTagsProperty =
        AvaloniaProperty.Register<TagInput, int>(nameof(MaxTags));

    public static readonly StyledProperty<bool> AllowDuplicatesProperty =
        AvaloniaProperty.Register<TagInput, bool>(nameof(AllowDuplicates));

    private static readonly char[] Separators = { ',', ';', '\n', '\t' };

    private readonly WrapLayout _flow;
    private readonly TextBox _textBox;

    static TagInput()
    {
        TagsProperty.Changed.AddClassHandler<TagInput>((t, e) => t.OnTagsReplaced(e.OldValue, e.NewValue));
        PlaceholderProperty.Changed.AddClassHandler<TagInput>((t, _) => t.UpdatePlaceholder());
        IsEnabledProperty.Changed.AddClassHandler<TagInput>((t, _) => t.Opacity = t.IsEnabled ? 1.0 : 0.5);
    }

    public IList<string>? Tags
    {
        get => GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public int MaxTags
    {
        get => GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, value);
    }

    public bool AllowDuplicates
    {
        get => GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    // User input only: setting Tags, or calling Add / Remove, from code doesn't raise it.
    public event EventHandler<IReadOnlyList<string>>? TagsChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public TagInput()
    {
        _textBox = new TextBox
        {
            FontSize = 14,
            Height = 28,
            Margin = new Thickness(2),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ShellPlatform.StripNativeChrome(_textBox);
        // Tunnel, so Backspace is seen before the text box handles it.
        _textBox.AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                AddFromText();
                e.Handled = true;
            }
            else if (e.Key == Key.Back && string.IsNullOrEmpty(_textBox.Text) && Current.Count > 0)
            {
                ByUser(() => Remove(Current[^1]));
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);
        _textBox.TextChanged += (_, _) =>
        {
            // Typing or pasting a separator commits everything before it.
            if (_textBox.Text?.IndexOfAny(Separators) >= 0) AddFromText();
        };
        _textBox.GotFocus += (_, _) => this.Token(BorderBrushProperty, ShellToken.Ring);
        _textBox.LostFocus += (_, _) => this.Token(BorderBrushProperty, ShellToken.Input);

        _flow = new WrapLayout { Spacing = 0, LineSpacing = 0, LastChildFill = true, VerticalAlignment = VerticalAlignment.Center, Children = { _textBox } };

        Child = _flow;
        MinHeight = 40;
        Padding = new Thickness(8, 3);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Ibeam);
        this.Token(BorderBrushProperty, ShellToken.Input);
        UpdatePlaceholder();
    }

    // Clicking the empty part of the field focuses the text box.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsEffectivelyEnabled && !_textBox.IsFocused) _textBox.Focus();
    }

    private IReadOnlyList<string> Current => (IReadOnlyList<string>?)Tags?.ToList() ?? Array.Empty<string>();

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void Add(string tag)
    {
        var text = tag.Trim();
        if (text.Length == 0) return;
        var current = Current;
        if (MaxTags > 0 && current.Count >= MaxTags) return;
        if (!AllowDuplicates && current.Contains(text, StringComparer.OrdinalIgnoreCase)) return;
        // A new list each time, so a two-way binding sees the change.
        Tags = new List<string>(current) { text };
    }

    public void Remove(string tag)
    {
        var next = new List<string>(Current);
        if (next.Remove(tag)) Tags = next;
    }

    private void AddFromText() => ByUser(AddTypedText);

    private void AddTypedText()
    {
        var text = _textBox.Text ?? string.Empty;
        _textBox.Text = string.Empty;
        foreach (var part in text.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            Add(part);
    }

    private void OnTagsReplaced(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnTagsMutated;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnTagsMutated;
        OnTagsMutated(this, null);
    }

    private void OnTagsMutated(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Rebuild();
        if (_byUser) TagsChanged?.Invoke(this, Current);
    }

    private void Rebuild()
    {
        _flow.Children.RemoveAll(_flow.Children.Where(c => c != _textBox).ToList());
        var index = 0;
        foreach (var tag in Current)
            _flow.Children.Insert(index++, CreateChip(tag));
        UpdatePlaceholder();
    }

    private Control CreateChip(string tag)
    {
        var label = new TextBlock { Text = tag, FontSize = 12, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        label.Token(TextBlock.ForegroundProperty, ShellToken.SecondaryForeground);
        var close = new Icon { Kind = IconName.X, Size = 12, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // 12px icon in a 20px click target.
        var remove = new Border { Width = 20, Height = 20, Background = Brushes.Transparent, Child = close, Cursor = new Cursor(StandardCursorType.Hand) };
        AutomationProperties.SetName(remove, $"Remove {tag}");
        remove.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        remove.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.PointerPressed += (_, e) => e.Handled = true;
        remove.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            if (IsEffectivelyEnabled) ByUser(() => Remove(tag));
            e.Handled = true;
        };

        var chip = new Border
        {
            Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, Children = { label, remove } },
            Padding = new Thickness(8, 0, 2, 0),
            Height = 24,
            Margin = new Thickness(2, 3),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Cursor = new Cursor(StandardCursorType.Arrow)
        };
        chip.Token(BackgroundProperty, ShellToken.Secondary);
        return chip;
    }

    private void UpdatePlaceholder() => _textBox.PlaceholderText = Current.Count == 0 ? Placeholder : null;
}
