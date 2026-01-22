using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardFooter component template
public static class CardFooterTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-footer",
        DisplayName = "Card Footer",
        Description = "Footer section for Card component, typically used for actions",
        Category = ComponentCategory.Layout,
        FilePath = "CardFooter.cs",
        Dependencies = new List<string>(),
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "footer", "actions" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

// Card footer section for actions
public partial class CardFooter : ContentView
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CardFooter), 
            StackOrientation.Horizontal, propertyChanged: OnOrientationChanged);

    public static readonly BindableProperty JustifyProperty =
        BindableProperty.Create(nameof(Justify), typeof(FooterJustify), typeof(CardFooter), 
            FooterJustify.End, propertyChanged: OnJustifyChanged);

    private readonly FlexLayout _flexLayout;

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public FooterJustify Justify
    {
        get => (FooterJustify)GetValue(JustifyProperty);
        set => SetValue(JustifyProperty, value);
    }

    public CardFooter()
    {
        _flexLayout = new FlexLayout
        {
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustify.End,
            AlignItems = FlexAlignItems.Center,
            Padding = new Thickness(16, 8, 16, 16)
        };

        // Add separator line at top
        var separator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb(""#E5E7EB""),
            HorizontalOptions = LayoutOptions.Fill
        };

        var stack = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { separator, _flexLayout }
        };

        Content = stack;
    }

    /*
     * Allow children to be added to the flex layout
     */
    public IList<IView> Children => _flexLayout.Children;

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CardFooter footer)
        {
            footer._flexLayout.Direction = (StackOrientation)newValue == StackOrientation.Horizontal 
                ? FlexDirection.Row 
                : FlexDirection.Column;
        }
    }

    private static void OnJustifyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CardFooter footer)
        {
            footer._flexLayout.JustifyContent = (FooterJustify)newValue switch
            {
                FooterJustify.Start => FlexJustify.Start,
                FooterJustify.Center => FlexJustify.Center,
                FooterJustify.End => FlexJustify.End,
                FooterJustify.SpaceBetween => FlexJustify.SpaceBetween,
                _ => FlexJustify.End
            };
        }
    }
}

public enum FooterJustify { Start, Center, End, SpaceBetween }
";
}
