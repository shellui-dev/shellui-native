using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Placeholder for an empty list or screen: icon in a muted circle, title, description and
// optional actions, centered. Bordered="True" adds shadcn's dashed outline.
//   <ui:EmptyState Icon="Folder" Title="No projects yet" Description="Create your first project to get started.">
//       <ui:Button Text="Create project" />
//   </ui:EmptyState>
[ContentProperty(nameof(Actions))]
public partial class EmptyState : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(EmptyState), IconName.Inbox,
            propertyChanged: (b, o, n) => ((EmptyState)b).Update());

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyState), string.Empty,
            propertyChanged: (b, o, n) => ((EmptyState)b).Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(EmptyState), string.Empty,
            propertyChanged: (b, o, n) => ((EmptyState)b).Update());

    public static readonly BindableProperty BorderedProperty =
        BindableProperty.Create(nameof(Bordered), typeof(bool), typeof(EmptyState), false,
            propertyChanged: (b, o, n) => ((EmptyState)b).Update());

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool Bordered
    {
        get => (bool)GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public IList<IView> Actions => _actions.Children;

    private readonly Border _frame;
    private readonly Icon _icon;
    private readonly Label _title;
    private readonly Label _description;
    private readonly HorizontalStackLayout _actions;

    public EmptyState()
    {
        _icon = new Icon { Size = 24, Token = ShellToken.MutedForeground };
        var badge = new Border
        {
            Content = _icon,
            WidthRequest = 48,
            HeightRequest = 48,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            HorizontalOptions = LayoutOptions.Center
        };
        badge.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);

        _title = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
        _title.Token(Label.TextColorProperty, ShellToken.Foreground);
        _description = new Label
        {
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            MaximumWidthRequest = 360
        };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        _actions = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };

        _frame = new Border
        {
            Content = new VerticalStackLayout { Spacing = 8, Children = { badge, _title, _description, _actions } },
            Padding = new Thickness(24, 32),
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            StrokeDashArray = new DoubleCollection { 4, 4 },
            BackgroundColor = Colors.Transparent
        };
        _frame.Token(Border.StrokeProperty, ShellToken.Border);
        Content = _frame;
        Update();
    }

    private void Update()
    {
        _icon.Name = Icon;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _frame.StrokeThickness = Bordered ? 1 : 0;
    }
}
