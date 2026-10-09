using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Placeholder for an empty list or screen: icon in a muted tile, title, description and
// optional actions, centered. Bordered="True" adds shadcn's dashed outline.
//   <ui:EmptyState Icon="Folder" Title="No projects yet" Description="Create your first project to get started.">
//       <ui:Button Text="Create project" />
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
