using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AvaloniaDemo.Components.UI;

// Loading placeholder — rounded-md bg-muted animate-pulse (opacity 1 → 0.5 → 1 every 2s).
// Sized by its parent; set Width, Height (default 20) and CornerRadius for other shapes.
public class Skeleton : Border
{
    private static readonly Animation Pulse = new()
    {
        Duration = TimeSpan.FromSeconds(1),
        IterationCount = IterationCount.Infinite,
        PlaybackDirection = PlaybackDirection.Alternate,
        Easing = new SineEaseInOut(),
        Children =
        {
            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 1.0) } },
            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 0.5) } }
        }
    };

    private CancellationTokenSource? _pulse;

    public Skeleton()
    {
        Height = 20;
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        this.Token(BackgroundProperty, ShellToken.Muted);
    }

    // Only animates while on screen.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _pulse?.Cancel();
        _pulse = new CancellationTokenSource();
        _ = Pulse.RunAsync(this, _pulse.Token);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _pulse?.Cancel();
        _pulse = null;
    }
}
