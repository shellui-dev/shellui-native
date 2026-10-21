using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardHeader component template
public static class CardHeaderTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-header",
        DisplayName = "Card Header",
        Description = "Header section for Card component with title and description",
        Category = ComponentCategory.Layout,
        FilePath = "CardHeader.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "header" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
// Views inside sit on the title's line (badges, a count) and wrap under it when there is no room;
// Action sits at the top right (shadcn's CardAction).
//   <ui:CardHeader Title=""Needs review"" Description=""Orders waiting for approval."">
//       <ui:Badge Text=""2"" Variant=""Destructive"" />
//       <ui:CardHeader.Action>
//           <ui:Button Text=""Download Excel"" Variant=""Outline"" Size=""Sm"" />
//       </ui:CardHeader.Action>
//   </ui:CardHeader>
[ContentProperty(nameof(Children))]
public partial class CardHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CardHeader), string.Empty,
            propertyChanged: (b, o, n) => (b as CardHeader)?.UpdateText());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(CardHeader), string.Empty,
            propertyChanged: (b, o, n) => (b as CardHeader)?.UpdateText());

    public static readonly BindableProperty ActionProperty =
        BindableProperty.Create(nameof(Action), typeof(View), typeof(CardHeader), null,
            propertyChanged: (b, o, n) => ((CardHeader)b).UpdateAction());

    private readonly Label _title;
    private readonly Label _description;
    private readonly FlexLayout _titleLine;
    private readonly ContentView _action;

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

    // Any view at the top right: a button, a menu, a select.
    public View? Action
    {
        get => (View?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    // Views after the title, on its line.
    public new IList<IView> Children { get; }

    public CardHeader()
    {
        _title = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold, IsVisible = false, Margin = new Thickness(0, 0, 8, 0) };
        _title.Token(Label.TextColorProperty, ShellToken.CardForeground);
        _description = new Label { FontSize = 14, IsVisible = false };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        // FlexLayout has no gap: the title and each item get 8 on their right.
        _titleLine = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center, Children = { _title } };
        Children = new TitleLine(_titleLine);
        _action = new ContentView { VerticalOptions = LayoutOptions.Start, IsVisible = false };

        var grid = new Grid
        {
            Padding = new Thickness(24, 24, 24, 16),
            ColumnSpacing = 16,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        grid.Add(new VerticalStackLayout { Spacing = 6, Children = { _titleLine, _description } }, 0, 0);
        grid.Add(_action, 1, 0);
        Content = grid;
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

    // The title line's items after the title label, spaced like gap-2.
    private sealed class TitleLine(FlexLayout line) : System.Collections.ObjectModel.Collection<IView>
    {
        protected override void InsertItem(int index, IView item)
        {
            base.InsertItem(index, item);
            if (item is View view && view.Margin == default) view.Margin = new Thickness(0, 2, 8, 2);
            line.Children.Insert(index + 1, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            line.Children.RemoveAt(index + 1);
        }

        protected override void SetItem(int index, IView item)
        {
            base.SetItem(index, item);
            line.Children[index + 1] = item;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            while (line.Children.Count > 1) line.Children.RemoveAt(1);
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
// Controls inside sit on the title's line (badges, a count) and wrap under it when there is no
// room; Action sits at the top right (shadcn's CardAction).
//   <ui:CardHeader Title=""Needs review"" Description=""Orders waiting for approval."">
//       <ui:Badge Text=""2"" Variant=""Destructive"" />
//       <ui:CardHeader.Action>
//           <ui:Button Text=""Download Excel"" Variant=""Outline"" Size=""Sm"" />
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

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto""), ColumnSpacing = 16 };
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
"
    };
}
