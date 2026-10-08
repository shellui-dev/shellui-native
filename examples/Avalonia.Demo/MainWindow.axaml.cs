using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaDemo.Components.UI;

namespace AvaloniaDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        foreach (var item in DemoDropdown.FindDescendantsOfType<DropdownItem>())
            item.Clicked += (_, _) => MenuChoice.Text = $"Picked {item.Text}.";

        var clicks = 0;
        LoadingButton.Clicked += async (_, _) =>
        {
            LoadingButton.IsLoading = true;
            Progress.Value = (Progress.Value + 20) % 120;
            await System.Threading.Tasks.Task.Delay(1500);
            LoadingButton.IsLoading = false;
            ClickCount.Text = $"Saved {++clicks} time(s).";
        };

        foreach (var token in Enum.GetValues<ShellToken>())
        {
            var chip = new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(ShellTheme.RadiusMd), BorderThickness = new Thickness(1) };
            chip.Token(Border.BackgroundProperty, token).Token(Border.BorderBrushProperty, ShellToken.Border);
            var label = new TextBlock { Text = token.ToString(), FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            Swatches.Children.Add(new StackPanel { Width = 120, Margin = new Thickness(0, 0, 8, 12), Spacing = 4, Children = { chip, label } });
        }

        foreach (var name in Enum.GetValues<IconName>().Where(n => n != IconName.None))
        {
            var label = new TextBlock { Text = name.ToString(), FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            Icons.Children.Add(new StackPanel { Width = 96, Margin = new Thickness(0, 0, 4, 12), Spacing = 6, Children = { new Icon { Kind = name, Size = 20 }, label } });
        }
    }
}
