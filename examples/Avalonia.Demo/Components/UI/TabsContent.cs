using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Panel shown while its Value is the active tab; fades in on switch.
public class TabsContent : Border
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<TabsContent, string?>(nameof(Value));

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TabsContent()
    {
        IsVisible = false;
    }

    internal void SetActive(bool active, bool animate)
    {
        if (IsVisible == active) return;
        Transitions = null;
        IsVisible = active;
        if (!active || !animate)
        {
            Opacity = 1;
            return;
        }
        Opacity = 0;
        Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(150), Easing = new CubicEaseOut() }
        };
        Opacity = 1;
    }
}
