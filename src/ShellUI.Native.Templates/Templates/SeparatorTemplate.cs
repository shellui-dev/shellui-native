using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Separator component template
public static class SeparatorTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "separator",
        DisplayName = "Separator",
        Description = "Visual divider/separator line for layout",
        Category = ComponentCategory.Layout,
        FilePath = "Separator.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string> { "horizontal", "vertical" },
        Tags = new List<string> { "layout", "divider", "separator", "line" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// 1px divider in the Border token.
public partial class Separator : ContentView
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(SeparatorOrientation), typeof(Separator),
            SeparatorOrientation.Horizontal, propertyChanged: OnOrientationChanged);

    private readonly BoxView _line;

    public SeparatorOrientation Orientation
    {
        get => (SeparatorOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Separator()
    {
        _line = new BoxView();
        _line.Token(BoxView.ColorProperty, ShellToken.Border);
        Content = _line;
        UpdateVisualState();
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Separator)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        if (Orientation == SeparatorOrientation.Horizontal)
        {
            _line.HeightRequest = 1;
            _line.WidthRequest = -1;
            _line.HorizontalOptions = LayoutOptions.Fill;
            _line.VerticalOptions = LayoutOptions.Center;
        }
        else
        {
            _line.WidthRequest = 1;
            _line.HeightRequest = -1;
            _line.HorizontalOptions = LayoutOptions.Center;
            _line.VerticalOptions = LayoutOptions.Fill;
        }
    }
}

public enum SeparatorOrientation
{
    Horizontal,
    Vertical
}
"
    };

}