using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AspectRatioTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "aspect-ratio",
        DisplayName = "Aspect Ratio",
        Description = "Keeps content at a fixed width / height ratio",
        Category = ComponentCategory.Layout,
        FilePath = "AspectRatio.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "aspect", "ratio", "layout", "media" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Keeps its content at a fixed width / height ratio: the height follows the width.
//   <ui:AspectRatio Ratio=""1.7778"">
//       <Image Source=""cover.jpg"" Aspect=""AspectFill"" />
//   </ui:AspectRatio>
public partial class AspectRatio : ContentView
{
    // Width divided by height. 16:9 is 1.7778, 4:3 is 1.3333, a square is 1.
    public static readonly BindableProperty RatioProperty =
        BindableProperty.Create(nameof(Ratio), typeof(double), typeof(AspectRatio), 16d / 9d,
            propertyChanged: (b, o, n) => ((AspectRatio)b).Fit());

    public double Ratio
    {
        get => (double)GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    public AspectRatio()
    {
        IsClippedToBounds = true;
        SizeChanged += (_, _) => Fit();
    }

    private void Fit()
    {
        if (Width <= 0 || Ratio <= 0) return;
        var height = Width / Ratio;
        // Only when it actually changes: setting it again would ask for another layout pass.
        if (Math.Abs(HeightRequest - height) > 0.5) HeightRequest = height;
    }
}
"
    };
}
