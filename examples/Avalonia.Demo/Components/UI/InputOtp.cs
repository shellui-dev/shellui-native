using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;

namespace AvaloniaDemo.Components.UI;

// One-time-code input — a row of 40px slots (rounded-md border border-input) over one invisible
// text box, so typing, paste and input methods all work. The active slot shows the ring color
// and a blinking caret.
// Usage: <ui:InputOtp Length="6" Value="{Binding Code}" Completed="OnCode" />
public class InputOtp : Border
{
    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<InputOtp, int>(nameof(Length), 6);

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<InputOtp, string?>(nameof(Value), string.Empty, defaultBindingMode: BindingMode.TwoWay);

    // Digits only. False allows letters too.
    public static readonly StyledProperty<bool> IsNumericProperty =
        AvaloniaProperty.Register<InputOtp, bool>(nameof(IsNumeric), true);

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<InputOtp, bool>(nameof(HasError));

    private const double SlotSize = 40;

    private readonly TextBox _textBox;
    private readonly StackPanel _slots = new() { Orientation = Orientation.Horizontal, Spacing = 8, IsHitTestVisible = false };
    private readonly List<(Border Box, TextBlock Text, Border Caret)> _items = new();
    private DispatcherTimer? _blink;
    private bool _focused;

    static InputOtp()
    {
        LengthProperty.Changed.AddClassHandler<InputOtp>((o, _) => o.BuildSlots());
        ValueProperty.Changed.AddClassHandler<InputOtp>((o, _) => o.OnValueChanged());
        IsNumericProperty.Changed.AddClassHandler<InputOtp>((o, _) => o.OnValueChanged());
        HasErrorProperty.Changed.AddClassHandler<InputOtp>((o, _) => o.UpdateSlots());
        IsEnabledProperty.Changed.AddClassHandler<InputOtp>((o, _) => o.Opacity = o.IsEnabled ? 1.0 : 0.5);
    }

    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsNumeric
    {
        get => GetValue(IsNumericProperty);
        set => SetValue(IsNumericProperty, value);
    }

    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public event EventHandler<string>? ValueChanged;
    // Raised when every slot is filled.
    public event EventHandler<string>? Completed;

    public InputOtp()
    {
        // The real input: stretched over the slots and invisible, so a click anywhere focuses it.
        _textBox = new TextBox { Opacity = 0, Cursor = new Cursor(StandardCursorType.Ibeam) };
        ShellPlatform.StripNativeChrome(_textBox);
        AutomationProperties.SetName(_textBox, "Verification code");
        _textBox.TextChanged += (_, _) =>
        {
            // Characters that don't belong (or don't fit) never stay in the hidden text.
            var value = Sanitize(_textBox.Text);
            if (_textBox.Text != value)
            {
                _textBox.Text = value;
                _textBox.CaretIndex = value.Length;
            }
            Value = value;
        };
        _textBox.GotFocus += (_, _) =>
        {
            _focused = true;
            _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
            UpdateSlots();
        };
        _textBox.LostFocus += (_, _) =>
        {
            _focused = false;
            UpdateSlots();
        };

        Child = new Panel { Children = { _slots, _textBox } };
        HorizontalAlignment = HorizontalAlignment.Left;
        BuildSlots();
    }

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None)
        => _textBox.Focus(method, keyModifiers);

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopBlink();
    }

    private int SlotCount => Math.Max(1, Length);

    private string Sanitize(string? text)
    {
        var filtered = new string((text ?? string.Empty).Where(c => IsNumeric ? char.IsDigit(c) : char.IsLetterOrDigit(c)).ToArray());
        return filtered.Length > SlotCount ? filtered[..SlotCount] : filtered;
    }

    private void BuildSlots()
    {
        _slots.Children.Clear();
        _items.Clear();
        for (var i = 0; i < SlotCount; i++)
        {
            var text = new TextBlock { FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            text.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
            var caret = new Border { Width = 1.5, Height = 18, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
            caret.Token(BackgroundProperty, ShellToken.Foreground);
            var box = new Border
            {
                Child = new Panel { Children = { text, caret } },
                Width = SlotSize,
                Height = SlotSize,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(ShellTheme.RadiusMd)
            };
            _items.Add((box, text, caret));
            _slots.Children.Add(box);
        }
        OnValueChanged();
    }

    private void OnValueChanged()
    {
        var value = Sanitize(Value);
        if (value != Value)
        {
            Value = value;
            return;
        }
        if (_textBox.Text != value)
        {
            _textBox.Text = value;
            _textBox.CaretIndex = value.Length;
        }
        UpdateSlots();
        ValueChanged?.Invoke(this, value);
        if (value.Length == Length && Length > 0) Completed?.Invoke(this, value);
    }

    private void UpdateSlots()
    {
        var value = Value ?? string.Empty;
        // The caret sits in the next empty slot (the last one when the code is complete).
        var active = _focused ? Math.Min(value.Length, _items.Count - 1) : -1;
        for (var i = 0; i < _items.Count; i++)
        {
            var (box, text, caret) = _items[i];
            text.Text = i < value.Length ? value[i].ToString() : string.Empty;
            var isActive = i == active;
            box.Token(BorderBrushProperty, HasError ? ShellToken.Destructive : isActive ? ShellToken.Ring : ShellToken.Input);
            caret.IsVisible = isActive && i >= value.Length;
            caret.Opacity = 1;
        }

        StopBlink();
        if (active < 0 || active < value.Length) return;
        var blinking = _items[active].Caret;
        _blink = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal,
            (_, _) => blinking.Opacity = blinking.Opacity > 0.5 ? 0 : 1);
        _blink.Start();
    }

    private void StopBlink()
    {
        _blink?.Stop();
        _blink = null;
    }
}
