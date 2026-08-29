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
        Dependencies = new List<string>(),
        Tags = new List<string> { "feedback", "skeleton", "loading", "placeholder" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

public partial class Skeleton : ContentView
{
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(Skeleton), 4.0,
            propertyChanged: (b, o, n) => (b as Skeleton)?.UpdateShape());

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
            BackgroundColor = Color.FromArgb(""#E5E7EB""),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 4 }
        };
        Content = _box;
        StartPulse();
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        // Restart the animation on reparenting so it survives navigation between pages.
        if (Parent != null) StartPulse();
        else this.AbortAnimation(""pulse"");
    }

    private void StartPulse()
    {
        // Opacity loop between 1.0 and 0.5 — read-only visual, no timing side effects.
        var anim = new Animation(v => _box.Opacity = v, 1.0, 0.5, Easing.SinInOut);
        anim.Commit(this, ""pulse"", 16, 1200, Easing.SinInOut, finished: null, repeat: () => true);
    }

    private void UpdateShape()
    {
        _box.StrokeShape = new RoundRectangle { CornerRadius = (float)CornerRadius };
    }
}
"
    };
}
