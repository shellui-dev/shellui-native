using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class EmptyStateTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "empty-state",
        DisplayName = "Empty State",
        Description = "Placeholder for an empty list or screen",
        Category = ComponentCategory.DataDisplay,
        FilePath = "EmptyState.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "empty", "placeholder", "state" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Placeholder for an empty list or screen: icon in a muted circle, title, description and
// optional actions, centered. Bordered=""True"" adds shadcn's dashed outline.
//   <ui:EmptyState Icon=""Folder"" Title=""No projects yet"" Description=""Create your first project to get started."">
//       <ui:Button Text=""Create project"" />
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
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Placeholder for an empty list or screen: icon in a muted tile, title, description and
// optional actions, centered. Bordered=""True"" adds shadcn's dashed outline.
//   <ui:EmptyState Icon=""Folder"" Title=""No projects yet"" Description=""Create your first project to get started."">
//       <ui:Button Text=""Create project"" />
//   </ui:EmptyState>
public class EmptyState : Border
{
    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<EmptyState, IconName>(nameof(Icon), IconName.Inbox);

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<bool> BorderedProperty =
        AvaloniaProperty.Register<EmptyState, bool>(nameof(Bordered));

    private readonly Rectangle _outline;
    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _description;
    private readonly StackPanel _actions;

    static EmptyState()
    {
        IconProperty.Changed.AddClassHandler<EmptyState>((s, _) => s.Update());
        TitleProperty.Changed.AddClassHandler<EmptyState>((s, _) => s.Update());
        DescriptionProperty.Changed.AddClassHandler<EmptyState>((s, _) => s.Update());
        BorderedProperty.Changed.AddClassHandler<EmptyState>((s, _) => s.Update());
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool Bordered
    {
        get => GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    // The action buttons, in a centered row.
    [Content]
    public Controls Actions => _actions.Children;

    public EmptyState()
    {
        _icon = new Icon { Size = 24, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var badge = new Border
        {
            Child = _icon,
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(ShellTheme.RadiusLg),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        badge.Token(BackgroundProperty, ShellToken.Muted);

        _title = new TextBlock { FontSize = 18, FontWeight = FontWeight.SemiBold, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _description = new TextBlock
        {
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 360
        };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        _actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) };

        // Border can't dash its stroke, so the outline is a shape behind the content.
        _outline = new Rectangle
        {
            StrokeThickness = 1,
            StrokeDashArray = new AvaloniaList<double> { 4, 4 },
            RadiusX = ShellTheme.RadiusLg,
            RadiusY = ShellTheme.RadiusLg
        };
        _outline.Token(Shape.StrokeProperty, ShellToken.Border);

        var content = new StackPanel { Spacing = 8, Margin = new Thickness(24, 32), Children = { badge, _title, _description, _actions } };
        Child = new Panel { Children = { _outline, content } };
        Update();
    }

    private void Update()
    {
        _icon.Kind = Icon;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _outline.IsVisible = Bordered;
    }
}
"
    };
}
