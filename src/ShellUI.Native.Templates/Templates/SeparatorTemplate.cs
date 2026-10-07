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
        _line = new BoxView { BackgroundColor = Colors.Transparent };
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
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// 1px divider in the Border token.
public class Separator : Border
{
    public static readonly StyledProperty<SeparatorOrientation> OrientationProperty =
        AvaloniaProperty.Register<Separator, SeparatorOrientation>(nameof(Orientation));

    static Separator()
    {
        OrientationProperty.Changed.AddClassHandler<Separator>((s, _) => s.UpdateVisualState());
    }

    public SeparatorOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Separator()
    {
        this.Token(BackgroundProperty, ShellToken.Border);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var horizontal = Orientation == SeparatorOrientation.Horizontal;
        Height = horizontal ? 1 : double.NaN;
        Width = horizontal ? double.NaN : 1;
        HorizontalAlignment = horizontal ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
        VerticalAlignment = horizontal ? VerticalAlignment.Center : VerticalAlignment.Stretch;
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