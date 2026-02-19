using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PopoverTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "popover",
        DisplayName = "Popover",
        Description = "Floating popover - use with PopoverTrigger and PopoverContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Popover.cs",
        Dependencies = new List<string> { "element-extensions", "popover-trigger", "popover-content" },
        Tags = new List<string> { "overlay", "popover", "floating" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

public partial class Popover : Grid
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Popover), false,
            propertyChanged: (b, o, n) => (b as Popover)?.OnOpenChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    private readonly VerticalStackLayout _triggerSlot;
    private readonly Border _contentSlot;

    public Popover()
    {
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _triggerSlot = new VerticalStackLayout { Spacing = 0 };
        _contentSlot = new Border
        {
            IsVisible = false,
            BackgroundColor = Color.FromArgb(""#FFFFFF""),
            Stroke = Color.FromArgb(""#E5E7EB""),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(16),
            Margin = new Thickness(0, 4, 0, 0),
            MinimumWidthRequest = 200,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.End
        };
        Children.Add(_triggerSlot);
        Children.Add(_contentSlot);
        Grid.SetRow(_triggerSlot, 0);
        Grid.SetRow(_contentSlot, 1);
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (child == _triggerSlot || child == _contentSlot) return;
        Dispatcher.Dispatch(() =>
        {
            if (child is IView view)
            {
                Children.Remove(view);
                if (child is PopoverTrigger pt)
                    _triggerSlot.Children.Add(pt);
                else if (child is PopoverContent pc)
                    _contentSlot.Content = pc;
            }
        });
    }

    public void ToggleAsync() => SetOpen(!IsOpen);
    public void CloseAsync() => SetOpen(false);
    public void SetOpen(bool value)
    {
        if (IsOpen != value) { IsOpen = value; OnOpenChanged(); }
    }

    private void OnOpenChanged()
    {
        _contentSlot.IsVisible = IsOpen;
        IsOpenChanged?.Invoke(this, IsOpen);
    }
}
";
}
