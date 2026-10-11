using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class InputOtpTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "input-otp",
        DisplayName = "Input OTP",
        Description = "One-time-code input with a slot per character",
        Category = ComponentCategory.Form,
        FilePath = "InputOtp.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "otp", "code", "verification", "input" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// One-time-code input — a row of 40px slots (rounded-md border border-input) over one hidden
// text field, so paste, autofill and the platform keyboard all work. The active slot shows the
// ring color and a blinking caret.
// Usage: <ui:InputOtp Length=""6"" Value=""{Binding Code}"" Completed=""OnCode"" />
public partial class InputOtp : ContentView
{
    public static readonly BindableProperty LengthProperty =
        BindableProperty.Create(nameof(Length), typeof(int), typeof(InputOtp), 6,
            propertyChanged: (b, o, n) => ((InputOtp)b).BuildSlots());

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(InputOtp), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((InputOtp)b).OnValueChanged());

    // Digits only (numeric keyboard). False allows letters too.
    public static readonly BindableProperty IsNumericProperty =
        BindableProperty.Create(nameof(IsNumeric), typeof(bool), typeof(InputOtp), true,
            propertyChanged: (b, o, n) => ((InputOtp)b)._entry.Keyboard = (bool)n ? Keyboard.Numeric : Keyboard.Text);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(InputOtp), false,
            propertyChanged: (b, o, n) => ((InputOtp)b).UpdateSlots());

    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsNumeric
    {
        get => (bool)GetValue(IsNumericProperty);
        set => SetValue(IsNumericProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public event EventHandler<string>? ValueChanged;
    // Raised when every slot is filled.
    public event EventHandler<string>? Completed;

    private const double SlotSize = 40;

    private readonly Entry _entry;
    private readonly HorizontalStackLayout _slots;
    private readonly List<(Border Box, Label Text, BoxView Caret)> _items = new();
    private bool _focused;

    public InputOtp()
    {
        _slots = new HorizontalStackLayout { Spacing = 8, InputTransparent = true };

        // The real input: stretched over the slots and invisible (transparent, 1px text), so a
        // tap anywhere focuses it. Kept at full opacity — fully transparent views don't get
        // touches on iOS.
        _entry = new Entry
        {
            Keyboard = Keyboard.Numeric,
            FontSize = 1,
            TextColor = Colors.Transparent,
            BackgroundColor = Colors.Transparent,
            ClearButtonVisibility = ClearButtonVisibility.Never,
            IsTextPredictionEnabled = false,
            IsSpellCheckEnabled = false
        };
        ShellPlatform.StripNativeChrome(_entry);
        ShellPlatform.HideCaret(_entry);
        ShellFocus.Track(_entry);
        _entry.TextChanged += (_, e) => Value = Sanitize(e.NewTextValue);
        _entry.Focused += (_, _) => { _focused = true; UpdateSlots(); };
        _entry.Unfocused += (_, _) => { _focused = false; UpdateSlots(); };

        Content = new Grid { Children = { _slots, _entry } };
        HorizontalOptions = LayoutOptions.Start;
        SemanticProperties.SetDescription(_entry, ""Verification code"");
        BuildSlots();

        Loaded += (_, _) => UpdateSlots();
        Unloaded += (_, _) => this.AbortAnimation(""OtpCaret"");
    }

    public new void Focus() => _entry.Focus();

    private string Sanitize(string? text)
    {
        var filtered = new string((text ?? string.Empty).Where(c => IsNumeric ? char.IsDigit(c) : char.IsLetterOrDigit(c)).ToArray());
        return filtered.Length > Length ? filtered[..Length] : filtered;
    }

    private void BuildSlots()
    {
        _slots.Children.Clear();
        _items.Clear();
        _entry.MaxLength = Math.Max(1, Length);
        for (var i = 0; i < Math.Max(1, Length); i++)
        {
            var text = new Label
            {
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            text.Token(Label.TextColorProperty, ShellToken.Foreground);
            var caret = new BoxView
            {
                WidthRequest = 1.5,
                HeightRequest = 18,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false
            };
            caret.Token(BoxView.ColorProperty, ShellToken.Foreground);
            var box = new Border
            {
                Content = new Grid { Children = { text, caret } },
                WidthRequest = SlotSize,
                HeightRequest = SlotSize,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
                BackgroundColor = Colors.Transparent
            };
            _items.Add((box, text, caret));
            _slots.Children.Add(box);
        }
        OnValueChanged();
    }

    private void OnValueChanged()
    {
        var value = Sanitize(Value);
        if (value != Value) { Value = value; return; }
        if (_entry.Text != value) _entry.Text = value;
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
            box.Token(Border.StrokeProperty, HasError ? ShellToken.Destructive : isActive ? ShellToken.Ring : ShellToken.Input);
            caret.IsVisible = isActive && i >= value.Length;
        }

        this.AbortAnimation(""OtpCaret"");
        if (active >= 0 && active >= value.Length)
        {
            var caret = _items[active].Caret;
            new Animation(v => caret.Opacity = v < 0.5 ? 1 : 0)
                .Commit(this, ""OtpCaret"", 16, 1000, Easing.Linear, repeat: () => _focused);
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;

namespace YourProjectNamespace.Components.UI;

// One-time-code input — a row of 40px slots (rounded-md border border-input) over one invisible
// text box, so typing, paste and input methods all work. The active slot shows the ring color
// and a blinking caret.
// Usage: <ui:InputOtp Length=""6"" Value=""{Binding Code}"" Completed=""OnCode"" />
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
        AutomationProperties.SetName(_textBox, ""Verification code"");
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
"
    };
}
