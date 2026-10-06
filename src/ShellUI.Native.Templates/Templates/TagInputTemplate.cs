using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TagInputTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "tag-input",
        DisplayName = "Tag Input",
        Description = "Text field that turns entries into removable tags",
        Category = ComponentCategory.Form,
        FilePath = "TagInput.cs",
        Dependencies = new List<string> { "shell", "icon", "wrap-layout" },
        Tags = new List<string> { "tags", "chips", "input", "labels" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Tag field — min-h-10 rounded-md border holding chips (rounded-md bg-secondary text-xs) and a
// text field that wraps with them. Enter, a comma or a semicolon turns what was typed into a
// tag; the × on a chip removes it.
// Usage: <ui:TagInput Tags=""{Binding Labels}"" Placeholder=""Add label..."" MaxTags=""5"" />
public partial class TagInput : ContentView
{
    public static readonly BindableProperty TagsProperty =
        BindableProperty.Create(nameof(Tags), typeof(IList<string>), typeof(TagInput), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((TagInput)b).OnTagsReplaced(o, n));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(TagInput), ""Add tag..."",
            propertyChanged: (b, o, n) => ((TagInput)b).UpdatePlaceholder());

    // 0 means no limit.
    public static readonly BindableProperty MaxTagsProperty =
        BindableProperty.Create(nameof(MaxTags), typeof(int), typeof(TagInput), 0);

    public static readonly BindableProperty AllowDuplicatesProperty =
        BindableProperty.Create(nameof(AllowDuplicates), typeof(bool), typeof(TagInput), false);

    public IList<string>? Tags
    {
        get => (IList<string>?)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public int MaxTags
    {
        get => (int)GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, value);
    }

    public bool AllowDuplicates
    {
        get => (bool)GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    public event EventHandler<IReadOnlyList<string>>? TagsChanged;

    private static readonly char[] Separators = { ',', ';', '\n', '\t' };

    private readonly Border _frame;
    private readonly WrapLayout _flow;
    private readonly Entry _entry;

    public TagInput()
    {
        _entry = new Entry
        {
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            HeightRequest = 28,
            MinimumHeightRequest = 28, // Android gives text fields a 44dp minimum otherwise
            Margin = new Thickness(2, 2),
            ClearButtonVisibility = ClearButtonVisibility.Never,
            IsTextPredictionEnabled = false
        };
        _entry.Token(Entry.TextColorProperty, ShellToken.Foreground);
        _entry.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_entry);
        ShellFocus.Track(_entry);
        _entry.Completed += (_, _) =>
        {
            AddFromEntry();
            KeepTyping();
        };
        _entry.TextChanged += (_, e) =>
        {
            // Typing or pasting a separator commits everything before it.
            if (e.NewTextValue?.IndexOfAny(Separators) >= 0) AddFromEntry();
        };
        _entry.Focused += (_, _) => _frame!.Token(Border.StrokeProperty, ShellToken.Ring);
        _entry.Unfocused += (_, _) => _frame!.Token(Border.StrokeProperty, ShellToken.Input);

        _flow = new WrapLayout { Spacing = 0, LineSpacing = 0, LastChildFill = true, Children = { _entry } };

        _frame = new Border
        {
            Content = _flow,
            MinimumHeightRequest = 40,
            Padding = new Thickness(8, 3),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _frame.Token(Border.StrokeProperty, ShellToken.Input);
        // Tapping the empty part of the field focuses the text entry.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsEnabled) _entry.Focus(); };
        _frame.GestureRecognizers.Add(tap);

        Content = _frame;
        UpdatePlaceholder();
    }

    private IReadOnlyList<string> Current => (IReadOnlyList<string>?)Tags?.ToList() ?? Array.Empty<string>();

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

    // Enter dismisses the soft keyboard; stay in the field so tags can be entered in a row.
    private void KeepTyping()
    {
        Dispatcher.Dispatch(async () =>
        {
            try
            {
                _entry.Focus();
                await _entry.ShowSoftInputAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                // No soft keyboard here, or the field is gone.
            }
        });
    }

    private void AddFromEntry()
    {
        var text = _entry.Text ?? string.Empty;
        _entry.Text = string.Empty;
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
        TagsChanged?.Invoke(this, Current);
    }

    private void Rebuild()
    {
        foreach (var chip in _flow.Children.Where(c => c != _entry).ToList())
            _flow.Children.Remove(chip);
        var index = 0;
        foreach (var tag in Current)
            _flow.Children.Insert(index++, CreateChip(tag));
        UpdatePlaceholder();
    }

    private View CreateChip(string tag)
    {
        var label = new Label { Text = tag, FontSize = 12, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.SecondaryForeground);
        var close = new Icon { Name = IconName.X, Size = 12, Token = ShellToken.MutedForeground };
        // 12px icon in a 20px tap target.
        var remove = new Grid { WidthRequest = 20, HeightRequest = 20, BackgroundColor = Colors.Transparent, Children = { close } };
        close.HorizontalOptions = LayoutOptions.Center;
        close.VerticalOptions = LayoutOptions.Center;
        SemanticProperties.SetDescription(remove, $""Remove {tag}"");
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        pointer.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsEnabled) Remove(tag); };
        remove.GestureRecognizers.Add(tap);

        var chip = new Border
        {
            Content = new HorizontalStackLayout { Spacing = 2, Children = { label, remove } },
            Padding = new Thickness(8, 0, 2, 0),
            HeightRequest = 24,
            Margin = new Thickness(2, 3),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        chip.Token(VisualElement.BackgroundColorProperty, ShellToken.Secondary);
        return chip;
    }

    private void UpdatePlaceholder() => _entry.Placeholder = Current.Count == 0 ? Placeholder : string.Empty;

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _entry != null)
        {
            Opacity = IsEnabled ? 1.0 : 0.5;
            _entry.IsEnabled = IsEnabled;
        }
    }
}
"
    };
}
