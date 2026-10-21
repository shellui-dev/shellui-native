using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
// Controls inside sit on the title's line (badges, a count) and wrap under it when there is no
// room; Action sits at the top right (shadcn's CardAction).
//   <ui:CardHeader Title="Needs review" Description="Orders waiting for approval.">
//       <ui:Badge Text="2" Variant="Destructive" />
//       <ui:CardHeader.Action>
//           <ui:Button Text="Download Excel" Variant="Outline" Size="Sm" />
//       </ui:CardHeader.Action>
//   </ui:CardHeader>
public class CardHeader : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Description));

    public static readonly StyledProperty<Control?> ActionProperty =
        AvaloniaProperty.Register<CardHeader, Control?>(nameof(Action));

    private readonly TextBlock _title;
    private readonly TextBlock _description;
    private readonly WrapPanel _titleLine;
    private readonly ContentControl _action;

    static CardHeader()
    {
        TitleProperty.Changed.AddClassHandler<CardHeader>((h, _) => h.UpdateText());
        DescriptionProperty.Changed.AddClassHandler<CardHeader>((h, _) => h.UpdateText());
        ActionProperty.Changed.AddClassHandler<CardHeader>((h, _) => h.UpdateAction());
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

    // Any control at the top right: a button, a menu, a select.
    public Control? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    // Controls after the title, on its line.
    [Content]
    public Controls Items { get; } = new();

    public CardHeader()
    {
        _title = new TextBlock { FontSize = 18, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.CardForeground);
        _description = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap, IsVisible = false };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        _titleLine = new WrapPanel { ItemSpacing = 8, LineSpacing = 4, Children = { _title } };
        Items.CollectionChanged += (_, _) =>
        {
            while (_titleLine.Children.Count > 1) _titleLine.Children.RemoveAt(1);
            foreach (var item in Items)
            {
                item.VerticalAlignment = VerticalAlignment.Center;
                _titleLine.Children.Add(item);
            }
        };
        _action = new ContentControl { VerticalAlignment = VerticalAlignment.Top, IsVisible = false };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 16 };
        var text = new StackPanel { Spacing = 6, Children = { _titleLine, _description } };
        grid.Children.Add(text);
        Grid.SetColumn(_action, 1);
        grid.Children.Add(_action);
        Child = grid;
        Padding = new Thickness(24, 24, 24, 16);
    }

    private void UpdateText()
    {
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
    }

    private void UpdateAction()
    {
        _action.Content = Action;
        _action.IsVisible = Action != null;
    }
}
