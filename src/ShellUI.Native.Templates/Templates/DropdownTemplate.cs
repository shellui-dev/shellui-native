using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DropdownTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dropdown",
        DisplayName = "Dropdown",
        Description = "Dropdown menu - use with DropdownTrigger, DropdownContent, DropdownItem",
        Category = ComponentCategory.Overlay,
        FilePath = "Dropdown.cs",
        Dependencies = new List<string> { "element-extensions", "dropdown-trigger", "dropdown-content", "dropdown-item" },
        Tags = new List<string> { "overlay", "dropdown", "menu" }
    };

    public static string Content => @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

public partial class Dropdown : Grid
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Dropdown), false,
            propertyChanged: (b, o, n) => (b as Dropdown)?.OnOpenChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    private readonly VerticalStackLayout _triggerSlot;
    private readonly Border _contentSlot;

    public Dropdown()
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
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Padding = new Thickness(4),
            Margin = new Thickness(0, 4, 0, 0),
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
                if (child is DropdownTrigger dt)
                    _triggerSlot.Children.Add(dt);
                else if (child is DropdownContent dc)
                    _contentSlot.Content = dc;
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
