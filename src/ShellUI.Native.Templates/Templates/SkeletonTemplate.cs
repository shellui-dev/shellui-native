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
"
    };
}
