using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// A row of toggles that work as one control: pick one (default) or several (Multiple).
//   <ui:ToggleGroup Value="{Binding View}">
//       <ui:ToggleGroupItem Value="day" Text="Day" />
//       <ui:ToggleGroupItem Value="week" Text="Week" />
//   </ui:ToggleGroup>
//   <ui:ToggleGroup Multiple="True" Variant="Outline" ValuesChanged="OnFormat"> ... </ui:ToggleGroup>
public class ToggleGroup : Border
{
    public static readonly StyledProperty<bool> MultipleProperty =
        AvaloniaProperty.Register<ToggleGroup, bool>(nameof(Multiple));

    // The pressed item's Value in single mode; null when nothing is pressed.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<ToggleGroup, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    // The pressed items' Values in Multiple mode.
    public static readonly StyledProperty<IList<string>?> ValuesProperty =
        AvaloniaProperty.Register<ToggleGroup, IList<string>?>(nameof(Values), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ToggleGroupVariant> VariantProperty =
        AvaloniaProperty.Register<ToggleGroup, ToggleGroupVariant>(nameof(Variant));

    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4 };

    static ToggleGroup()
    {
        MultipleProperty.Changed.AddClassHandler<ToggleGroup>((g, _) => g.Refresh());
        VariantProperty.Changed.AddClassHandler<ToggleGroup>((g, _) => g.Refresh());
        ValueProperty.Changed.AddClassHandler<ToggleGroup>((g, _) =>
        {
            g.Refresh();
            if (g._byUser) g.ValueChanged?.Invoke(g, g.Value);
        });
        ValuesProperty.Changed.AddClassHandler<ToggleGroup>((g, e) => g.OnValuesReplaced(e.OldValue, e.NewValue));
    }

    public bool Multiple
    {
        get => GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IList<string>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public ToggleGroupVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    [Content]
    public Controls Items => _row.Children;

    // User input only: setting Value or Values from code or a binding doesn't raise them.
    public event EventHandler<string?>? ValueChanged;
    public event EventHandler<IReadOnlyList<string>>? ValuesChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public ToggleGroup()
    {
        _row.Children.CollectionChanged += (_, _) => Refresh();
        Child = _row;
        HorizontalAlignment = HorizontalAlignment.Left;
    }

    public bool IsPressed(string? value) =>
        value != null && (Multiple ? Values?.Contains(value) == true : Value == value);

    // A click or key on an item.
    internal void Toggle(string? value) => ByUser(() => ToggleValue(value));

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private void ToggleValue(string? value)
    {
        if (value is null || !IsEffectivelyEnabled) return;
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

    private void OnValuesReplaced(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnValuesMutated;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnValuesMutated;
        OnValuesMutated(this, null);
    }

    private void OnValuesMutated(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        Refresh();
        if (_byUser) ValuesChanged?.Invoke(this, (Values ?? Array.Empty<string>()).ToList());
    }

    // Items don't track the group's state: the group pushes it to them.
    private void Refresh()
    {
        foreach (var item in _row.Children.OfType<ToggleGroupItem>())
            item.Apply(IsPressed(item.Value), Variant == ToggleGroupVariant.Outline);
    }
}

// One toggle in a ToggleGroup — h-9 min-w-9 rounded-md px-2.5, bg-accent while pressed. A tab
// stop that toggles with Enter or Space.
public class ToggleGroupItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<ToggleGroupItem, string?>(nameof(Value));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ToggleGroupItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<ToggleGroupItem, IconName>(nameof(Icon));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private bool _pressed;
    private bool _outline;

    static ToggleGroupItem()
    {
        TextProperty.Changed.AddClassHandler<ToggleGroupItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<ToggleGroupItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<ToggleGroupItem>((i, _) => i.Update());
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ToggleGroupItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _icon, _label }
        };
        Height = 36;
        MinWidth = 36;
        Padding = new Thickness(10, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        Update();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Update();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Update();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Press();
        e.Handled = true;
    }

    private void Press()
    {
        if (IsEffectivelyEnabled) this.FindParentOfType<ToggleGroup>()?.Toggle(Value);
    }

    internal void Apply(bool pressed, bool outline)
    {
        _pressed = pressed;
        _outline = outline;
        Update();
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        AutomationProperties.SetName(this, string.IsNullOrEmpty(Text) ? Icon.ToString() : Text);

        BorderThickness = new Thickness(_outline ? 1 : 0);
        this.Token(BorderBrushProperty, ShellToken.Input);

        var hovered = IsPointerOver && IsEffectivelyEnabled;
        var foreground = _pressed ? ShellToken.AccentForeground
            : hovered ? ShellToken.MutedForeground
            : ShellToken.Foreground;
        _label.Token(TextBlock.ForegroundProperty, foreground);
        _icon.Token = foreground;

        if (_pressed) this.Token(BackgroundProperty, ShellToken.Accent);
        else if (hovered) this.Token(BackgroundProperty, ShellToken.Muted);
        else
        {
            // Transparent, not null, so the whole toggle stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}

public enum ToggleGroupVariant { Default, Outline }
