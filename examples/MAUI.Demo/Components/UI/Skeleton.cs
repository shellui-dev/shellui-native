using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
        Unloaded += (_, _) => this.AbortAnimation("pulse");
    }

    private void StartPulse()
    {
        if (this.AnimationIsRunning("pulse")) return;
        new Animation(v => _box.Opacity = 1 - 0.5 * Math.Sin(Math.PI * v))
            .Commit(this, "pulse", 16, 2000, Easing.Linear, repeat: () => true);
    }
}
