using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Card body — p-6 pt-0.
public class CardContent : Border
{
    public CardContent() => Padding = new Thickness(24, 0, 24, 24);
}
