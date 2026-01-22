using MauiIcons.Core;
using MauiIcons.Fluent;

namespace MAUI.Demo.Components.UI;

// Icon component wrapper for MauiIcons Fluent
// Cross-platform icons: https://github.com/AathifMahir/MauiIcons
// Usage: <ui:Icon IconType="{x:Static ui:Icons.Search}" Size="24" />
public partial class Icon : ContentView
{
    public static readonly BindableProperty IconTypeProperty =
        BindableProperty.Create(nameof(IconType), typeof(FluentIcons?), typeof(Icon), null, 
            propertyChanged: OnIconChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Icon), 20.0, 
            propertyChanged: OnSizeChanged);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(Icon), null, 
            propertyChanged: OnColorChanged);

    private readonly MauiIcon _mauiIcon;

    public FluentIcons? IconType
    {
        get => (FluentIcons?)GetValue(IconTypeProperty);
        set => SetValue(IconTypeProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Color? IconColor
    {
        get => (Color?)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public Icon()
    {
        _mauiIcon = new MauiIcon
        {
            IconSize = 20,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        Content = _mauiIcon;

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += (s, e) => UpdateColor();
        
        UpdateColor();
    }

    private static void OnIconChanged(BindableObject b, object o, object n)
    {
        if (b is Icon icon && n is FluentIcons fluentIcon)
            icon._mauiIcon.Icon = fluentIcon;
    }

    private static void OnSizeChanged(BindableObject b, object o, object n)
    {
        if (b is Icon icon)
            icon._mauiIcon.IconSize = (double)n;
    }

    private static void OnColorChanged(BindableObject b, object o, object n)
    {
        if (b is Icon icon)
            icon.UpdateColor();
    }

    private void UpdateColor()
    {
        _mauiIcon.IconColor = IconColor ?? ShellTheme.Foreground;
    }
}
