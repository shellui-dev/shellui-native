using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI;
using AvaloniaDemo.Components.UI.Variants;
using Xunit;
using Button = AvaloniaDemo.Components.UI.Button;

namespace ShellUI.Native.Avalonia.Tests;

// Slots and states apps asked for: password reveal, cell views, inline links, card header
// actions, stat card tone / loading, and theme tokens on text runs.
public class SlotAndStateTests
{
    private static Border RevealButton(Control input) =>
        input.GetVisualDescendants().OfType<Border>().Single(b => AutomationProperties.GetName(b) is "Show password" or "Hide password");

    private static TextBox TextBoxOf(Control input) => input.GetVisualDescendants().OfType<TextBox>().Single();

    [AvaloniaFact]
    public void Password_variant_reveal_button_shows_and_hides_the_text()
    {
        var input = new Input { Text = "secret", Variant = InputVariant.Password, Width = 300 };
        var window = Ui.Show(input);
        var box = TextBoxOf(input);
        var reveal = RevealButton(input);
        Assert.True(reveal.IsVisible);
        Assert.Equal('•', box.PasswordChar);

        Ui.Click(window, reveal);
        Assert.Equal(default, box.PasswordChar);
        Assert.Equal("Hide password", AutomationProperties.GetName(reveal));
        Assert.True(box.IsFocused);

        Ui.Click(window, reveal);
        Assert.Equal('•', box.PasswordChar);
        Assert.Equal("Show password", AutomationProperties.GetName(reveal));
    }

    [AvaloniaFact]
    public void IsPassword_masks_without_a_button_and_the_variant_resets_when_changed()
    {
        var input = new Input { Text = "secret", IsPassword = true, Width = 300 };
        var window = Ui.Show(input);
        Assert.False(RevealButton(input).IsVisible);
        Assert.Equal('•', TextBoxOf(input).PasswordChar);

        input.Variant = InputVariant.Password;
        Ui.Settle();
        Ui.Click(window, RevealButton(input));
        Assert.Equal(default, TextBoxOf(input).PasswordChar);

        input.Variant = InputVariant.Default;
        Assert.False(RevealButton(input).IsVisible);
        Assert.Equal('•', TextBoxOf(input).PasswordChar);
        input.IsPassword = false;
        Assert.Equal(default, TextBoxOf(input).PasswordChar);
    }

    [AvaloniaFact]
    public void InputGroup_reveal_button_sits_after_the_suffix()
    {
        var group = new InputGroup { Text = "secret", Variant = InputVariant.Password, SuffixText = "kg", Width = 300 };
        var window = Ui.Show(group);
        var reveal = RevealButton(group);
        var suffix = group.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "kg");
        Assert.True(reveal.TranslatePoint(default, window)!.Value.X > suffix.TranslatePoint(default, window)!.Value.X);

        Ui.Click(window, reveal);
        Assert.Equal(default, TextBoxOf(group).PasswordChar);
    }

    private sealed record Row(string Name, string Status);

    [AvaloniaFact]
    public void DataTable_column_cell_draws_a_control_and_still_sorts_by_its_property()
    {
        var table = new DataTable { Width = 600 };
        table.Columns.Add(new DataTableColumn { Header = "Name", Property = "Name" });
        table.Columns.Add(new DataTableColumn { Header = "Status", Property = "Status", Cell = item => new Badge { Text = "badge:" + ((Row)item).Status } });
        table.ItemsSource = new List<Row> { new("a", "Pending"), new("b", "Active") };
        Ui.Show(table);

        string[] Badges() => table.GetVisualDescendants().OfType<Badge>().Select(b => b.Text!).ToArray();
        Assert.Equal(new[] { "badge:Pending", "badge:Active" }, Badges());
        Assert.DoesNotContain(table.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "Pending");

        table.SortBy(table.Columns[1]);
        Ui.Settle();
        Assert.Equal(new[] { "badge:Active", "badge:Pending" }, Badges());
    }

    [AvaloniaFact]
    public void Inline_button_has_no_padding_and_the_height_of_its_text()
    {
        var link = new Button { Text = "Sign in", Variant = ButtonVariant.Link, Size = ButtonSize.Inline };
        var normal = new Button { Text = "Sign in", Variant = ButtonVariant.Link };
        Ui.Show(new StackPanel { Children = { link, normal } });
        Assert.Equal(default, link.Padding);
        Assert.True(link.Bounds.Height < 24, $"height {link.Bounds.Height}");
        // Default is px-4: 32 wider for the same text.
        Assert.True(Math.Abs(normal.Bounds.Width - link.Bounds.Width - 32) < 1, $"{normal.Bounds.Width} vs {link.Bounds.Width}");
    }

    [AvaloniaFact]
    public void CardHeader_puts_children_on_the_title_line_and_the_action_on_the_right()
    {
        var badge = new Badge { Text = "2" };
        var action = new Button { Text = "Export" };
        var header = new CardHeader { Title = "Needs review", Description = "Waiting for approval", Action = action, Width = 420 };
        header.Items.Add(badge);
        var window = Ui.Show(header);

        var title = header.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == "Needs review");
        var description = header.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == "Waiting for approval");
        var titleAt = title.TranslatePoint(default, window)!.Value;
        var badgeAt = badge.TranslatePoint(default, window)!.Value;
        var actionAt = action.TranslatePoint(default, window)!.Value;
        var headerAt = header.TranslatePoint(default, window)!.Value;
        Assert.True(badgeAt.X > titleAt.X + title.Bounds.Width);
        Assert.True(badgeAt.Y < description.TranslatePoint(default, window)!.Value.Y);
        Assert.True(actionAt.X > badgeAt.X + badge.Bounds.Width);
        // p-6: the action ends 24 in from the header's right edge.
        Assert.True(Math.Abs(actionAt.X + action.Bounds.Width - headerAt.X - (420 - 24)) < 1, $"action ends at {actionAt.X + action.Bounds.Width - headerAt.X}");
    }

    [AvaloniaFact]
    public void CardHeader_without_a_title_has_no_empty_title_line()
    {
        var header = new CardHeader { Description = "Only a description" };
        Ui.Show(header);
        Assert.DoesNotContain(header.GetVisualDescendants().OfType<TextBlock>(), t => t.IsEffectivelyVisible && string.IsNullOrEmpty(t.Text));
    }

    [AvaloniaFact]
    public void StatCard_value_tone_follows_the_token()
    {
        var card = new StatCard { Title = "Last backup", Value = "Backup failed", ValueTone = StatTone.Destructive };
        Ui.Show(card);
        var value = card.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "Backup failed");
        Assert.Equal(ShellTheme.Light[ShellToken.Destructive], ((ISolidColorBrush)value.Foreground!).Color);

        Ui.Show(card = new StatCard { Value = "Backup failed", ValueTone = StatTone.Destructive }, ThemeVariant.Dark);
        value = card.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "Backup failed");
        Assert.Equal(ShellTheme.Dark[ShellToken.Destructive], ((ISolidColorBrush)value.Foreground!).Color);
    }

    [AvaloniaFact]
    public void StatCard_pill_wraps_under_a_long_value()
    {
        var card = new StatCard { Title = "Last backup", Value = "Backup failed", Change = "Action needed", Trend = StatTrend.Down, Width = 230 };
        var window = Ui.Show(card);
        var value = card.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "Backup failed");
        var change = card.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "Action needed");
        var valueBottom = value.TranslatePoint(new Point(0, value.Bounds.Height), window)!.Value.Y;
        Assert.True(change.TranslatePoint(default, window)!.Value.Y >= valueBottom - 1, $"value {value.TranslatePoint(default, window)} {value.Bounds.Size}, change {change.TranslatePoint(default, window)} {change.Bounds.Size}");
        Assert.True(change.TranslatePoint(new Point(change.Bounds.Width, 0), card)!.Value.X <= card.Bounds.Width);
    }

    [AvaloniaFact]
    public void StatCard_loading_shows_skeletons_in_place_of_the_value()
    {
        var card = new StatCard { Title = "Queue", Value = "12", Description = "waiting", IsLoading = true };
        Ui.Show(card);
        var value = card.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "12");
        Assert.False(value.IsEffectivelyVisible);
        Assert.Equal(2, card.GetVisualDescendants().OfType<Skeleton>().Count(s => s.IsEffectivelyVisible));

        card.IsLoading = false;
        Ui.Settle();
        Assert.True(value.IsEffectivelyVisible);
        Assert.DoesNotContain(card.GetVisualDescendants().OfType<Skeleton>(), s => s.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void StatCard_small_size_is_more_compact()
    {
        var normal = new StatCard { Title = "Sent", Value = "1,204", Width = 160 };
        var small = new StatCard { Title = "Sent", Value = "1,204", Size = StatCardSize.Sm, Width = 160 };
        Ui.Show(new StackPanel { Children = { normal, small } });
        Assert.True(small.Bounds.Height < normal.Bounds.Height);
        Assert.Equal(new Thickness(16), small.Padding);
    }

    [AvaloniaFact]
    public void Run_with_a_token_follows_the_theme()
    {
        var run = new Run(" · 2 min ago").Token(Run.ForegroundProperty, ShellToken.MutedForeground);
        var text = new TextBlock();
        text.Inlines!.Add(run);
        Ui.Show(text);
        Assert.Equal(ShellTheme.Light[ShellToken.MutedForeground], ((ISolidColorBrush)run.Foreground!).Color);

        ShellTheme.SetTheme(ThemeVariant.Dark);
        Ui.Settle();
        Assert.Equal(ShellTheme.Dark[ShellToken.MutedForeground], ((ISolidColorBrush)run.Foreground!).Color);
    }
}
