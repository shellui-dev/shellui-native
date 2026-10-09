using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Tab strip — inline-flex h-10 rounded-lg bg-muted p-1.
public class TabsList : Border
{
    private readonly StackPanel _stack = new() { Orientation = Orientation.Horizontal };

    [Content]
    public Controls Items => _stack.Children;

    public TabsList()
    {
        Child = _stack;
        Height = 40;
        Padding = new Thickness(4);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        HorizontalAlignment = HorizontalAlignment.Left;
        this.Token(BackgroundProperty, ShellToken.Muted);
    }
}
