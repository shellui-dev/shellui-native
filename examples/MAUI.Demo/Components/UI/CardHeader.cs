namespace MAUI.Demo.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
// Views inside sit on the title's line (badges, a count) and wrap under it when there is no room;
// Action sits at the top right (shadcn's CardAction).
//   <ui:CardHeader Title="Needs review" Description="Orders waiting for approval.">
//       <ui:Badge Text="2" Variant="Destructive" />
//       <ui:CardHeader.Action>
//           <ui:Button Text="Download Excel" Variant="Outline" Size="Sm" />
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
