using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Skeleton - pulsing grey placeholder for loading states.
public static class SkeletonTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "skeleton",
        DisplayName = "Skeleton",
        Description = "Loading placeholder - a pulsing grey block sized by parent. Use in lists while data loads",
        Category = ComponentCategory.Feedback,
        FilePath = "Skeleton.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "feedback", "skeleton", "loading", "placeholder" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Loading placeholder — rounded-md bg-muted animate-pulse (opacity 1 → 0.5 → 1 every 2s).
public partial class Skeleton : ContentView
{
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(Skeleton), (double)ShellTheme.RadiusMd,
            propertyChanged: (b, o, n) => ((Skeleton)b)._box.StrokeShape = new RoundRectangle { CornerRadius = (float)(double)n });

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private readonly Border _box;

    public Skeleton()
    {
        HeightRequest = 20;
        _box = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        _box.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        Content = _box;

        // Only animate while on screen.
        Loaded += (_, _) => StartPulse();
        Unloaded += (_, _) => this.AbortAnimation(""pulse"");
    }

    private void StartPulse()
    {
        if (this.AnimationIsRunning(""pulse"")) return;
        new Animation(v => _box.Opacity = 1 - 0.5 * Math.Sin(Math.PI * v))
            .Commit(this, ""pulse"", 16, 2000, Easing.Linear, repeat: () => true);
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;

namespace YourProjectNamespace.Components.UI;

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
"
    };
}
