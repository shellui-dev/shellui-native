using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI.Variants;

namespace AvaloniaDemo.Components.UI;

// Chat — a rounded-lg bordered panel: an optional header, a scrolling list of messages that
// follows the newest one, and an optional footer (usually a ChatInput). Give it a height.
//   <ui:Chat x:Name="Conversation" Height="420">
//       <ui:Chat.Header> ...any control... </ui:Chat.Header>
//       <ui:ChatMessage Role="Assistant" Sender="ShellUI" Text="How can I help?" AvatarText="S" />
//       <ui:ChatMessage Role="User" Text="Show me the button." />
//       <ui:Chat.Footer>
//           <ui:ChatInput Submitted="OnSend" />
//       </ui:Chat.Footer>
//   </ui:Chat>
// Add messages from code with Conversation.Messages.Add(new ChatMessage { ... }).
public class Chat : Border
{
    public static readonly StyledProperty<Control?> HeaderProperty =
        AvaloniaProperty.Register<Chat, Control?>(nameof(Header));

    public static readonly StyledProperty<Control?> FooterProperty =
        AvaloniaProperty.Register<Chat, Control?>(nameof(Footer));

    // Scrolls to the newest message when one is added.
    public static readonly StyledProperty<bool> AutoScrollProperty =
        AvaloniaProperty.Register<Chat, bool>(nameof(AutoScroll), true);

    private readonly StackPanel _messages = new() { Spacing = 16, Margin = new Thickness(16) };
    private readonly ScrollViewer _scroll;
    private readonly Border _header = new() { Padding = new Thickness(16, 12), BorderThickness = new Thickness(0, 0, 0, 1), IsVisible = false };
    private readonly Border _footer = new() { Padding = new Thickness(12), BorderThickness = new Thickness(0, 1, 0, 0), IsVisible = false };
    private double _lastHeight;

    static Chat()
    {
        HeaderProperty.Changed.AddClassHandler<Chat>((c, _) => c.UpdateSlots());
        FooterProperty.Changed.AddClassHandler<Chat>((c, _) => c.UpdateSlots());
    }

    public Control? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Control? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public bool AutoScroll
    {
        get => GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    // The ChatMessage children.
    [Content]
    public Controls Messages => _messages.Children;

    public Chat()
    {
        _scroll = new ScrollViewer { Content = _messages, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        _header.Token(BorderBrushProperty, ShellToken.Border);
        _footer.Token(BorderBrushProperty, ShellToken.Border);
        Grid.SetRow(_scroll, 1);
        Grid.SetRow(_footer, 2);

        Child = new Grid { RowDefinitions = new RowDefinitions("Auto,*,Auto"), Children = { _header, _scroll, _footer } };
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        ClipToBounds = true;
        this.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);

        _messages.Children.CollectionChanged += (_, _) => { if (AutoScroll) ScrollToEnd(); };
        // A message that grows (a reply still arriving) keeps the list at the end, unless the
        // reader has scrolled up.
        _messages.SizeChanged += (_, e) =>
        {
            var wasAtEnd = _scroll.Offset.Y >= _lastHeight - _scroll.Viewport.Height - 24;
            _lastHeight = e.NewSize.Height + 32;
            if (AutoScroll && wasAtEnd) ScrollToEnd();
        };
    }

    // Shows the newest message, once a message added just now has its size.
    public void ScrollToEnd() => Dispatcher.UIThread.Post(() => _scroll.ScrollToEnd(), DispatcherPriority.Loaded);

    private void UpdateSlots()
    {
        _header.Child = Header;
        _header.IsVisible = Header != null;
        _footer.Child = Footer;
        _footer.IsVisible = Footer != null;
    }
}

public enum ChatRole { Assistant, User, System }

// One message — an optional avatar beside a bubble (rounded-lg px-3 py-2 text-sm, at most 80%
// of the width). User messages sit on the right in bg-primary, assistant ones on the left in
// bg-muted, system ones are outlined and italic. Its child control goes inside the bubble.
public class ChatMessage : Border
{
    public static readonly StyledProperty<ChatRole> RoleProperty =
        AvaloniaProperty.Register<ChatMessage, ChatRole>(nameof(Role));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ChatMessage, string?>(nameof(Text));

    // The sender's name above the bubble. (Avalonia controls already have a Name.)
    public static readonly StyledProperty<string?> SenderProperty =
        AvaloniaProperty.Register<ChatMessage, string?>(nameof(Sender));

    public static readonly StyledProperty<string?> TimestampProperty =
        AvaloniaProperty.Register<ChatMessage, string?>(nameof(Timestamp));

    // Shows three bouncing dots after the text while a reply is still arriving.
    public static readonly StyledProperty<bool> IsStreamingProperty =
        AvaloniaProperty.Register<ChatMessage, bool>(nameof(IsStreaming));

    // Initials shown in the avatar circle.
    public static readonly StyledProperty<string?> AvatarTextProperty =
        AvaloniaProperty.Register<ChatMessage, string?>(nameof(AvatarText));

    // Any control for the avatar circle (an Avatar, an Icon); replaces AvatarText.
    public static readonly StyledProperty<Control?> AvatarProperty =
        AvaloniaProperty.Register<ChatMessage, Control?>(nameof(Avatar));

    // A control inside the bubble, under the text.
    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<ChatMessage, Control?>(nameof(Body));

    // A control under the bubble (copy, retry, ...).
    public static readonly StyledProperty<Control?> ActionsProperty =
        AvaloniaProperty.Register<ChatMessage, Control?>(nameof(Actions));

    private readonly Grid _row;
    private readonly Border _avatar;
    private readonly TextBlock _avatarText;
    private readonly StackPanel _column;
    private readonly StackPanel _meta;
    private readonly TextBlock _sender;
    private readonly TextBlock _timestamp;
    private readonly Border _bubble;
    private readonly TextBlock _text;
    private readonly Border _body = new();
    private readonly StackPanel _dots;
    private readonly Border _actions = new() { IsVisible = false };
    private DispatcherTimer? _timer;
    private double _phase;

    static ChatMessage()
    {
        RoleProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        TextProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        SenderProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        TimestampProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        IsStreamingProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        AvatarTextProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        AvatarProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        ActionsProperty.Changed.AddClassHandler<ChatMessage>((m, _) => m.Update());
        BodyProperty.Changed.AddClassHandler<ChatMessage>((m, e) => m._body.Child = e.NewValue as Control);
    }

    public ChatRole Role
    {
        get => GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Sender
    {
        get => GetValue(SenderProperty);
        set => SetValue(SenderProperty, value);
    }

    public string? Timestamp
    {
        get => GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    public bool IsStreaming
    {
        get => GetValue(IsStreamingProperty);
        set => SetValue(IsStreamingProperty, value);
    }

    public string? AvatarText
    {
        get => GetValue(AvatarTextProperty);
        set => SetValue(AvatarTextProperty, value);
    }

    public Control? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    [Content]
    public Control? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public Control? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public ChatMessage()
    {
        _avatarText = new TextBlock { FontSize = 12, FontWeight = FontWeight.Medium, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _avatarText.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        // h-8 w-8 rounded-full bg-muted, clipped to the circle.
        _avatar = new Border { Width = 32, Height = 32, CornerRadius = new CornerRadius(16), ClipToBounds = true, VerticalAlignment = VerticalAlignment.Top, IsVisible = false };
        _avatar.Token(BackgroundProperty, ShellToken.Muted);

        _sender = new TextBlock { FontSize = 12, FontWeight = FontWeight.Medium };
        _sender.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _timestamp = new TextBlock { FontSize = 12 };
        _timestamp.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _sender, _timestamp } };

        _text = new TextBlock { FontSize = 14, LineHeight = 20, TextWrapping = TextWrapping.Wrap };
        _dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3, Height = 14, IsVisible = false };
        for (var i = 0; i < 3; i++)
            _dots.Children.Add(new Border { Width = 6, Height = 6, CornerRadius = new CornerRadius(3), VerticalAlignment = VerticalAlignment.Bottom, RenderTransform = new TranslateTransform() });
        AutomationProperties.SetName(_dots, "Typing");

        _bubble = new Border
        {
            Child = new StackPanel { Spacing = 6, Children = { _text, _body, _dots } },
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(ShellTheme.RadiusLg)
        };
        _column = new StackPanel { Spacing = 4, Children = { _meta, _bubble, _actions } };
        Grid.SetColumn(_column, 1);

        _row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 12, Children = { _column, _avatar } };
        // max-w-[80%]
        _row.SizeChanged += (_, e) => _column.MaxWidth = Math.Max(120, e.NewSize.Width * 0.8);
        Child = _row;
        Update();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateDots();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopDots();
    }

    private void Update()
    {
        var user = Role == ChatRole.User;
        var side = user ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        _avatar.Child = Avatar ?? (string.IsNullOrEmpty(AvatarText) ? null : _avatarText);
        _avatarText.Text = AvatarText ?? string.Empty;
        _avatar.IsVisible = _avatar.Child != null;
        Grid.SetColumn(_avatar, user ? 2 : 0);

        _column.HorizontalAlignment = side;
        _meta.HorizontalAlignment = side;
        _bubble.HorizontalAlignment = side;
        _actions.HorizontalAlignment = side;

        _sender.Text = Sender ?? string.Empty;
        _sender.IsVisible = !string.IsNullOrEmpty(Sender);
        _timestamp.Text = Timestamp ?? string.Empty;
        _timestamp.IsVisible = !string.IsNullOrEmpty(Timestamp);
        _meta.IsVisible = _sender.IsVisible || _timestamp.IsVisible;

        var (background, foreground) = Role switch
        {
            ChatRole.User => (ShellToken.Primary, ShellToken.PrimaryForeground),
            ChatRole.System => (ShellToken.Background, ShellToken.MutedForeground),
            _ => (ShellToken.Muted, ShellToken.Foreground)
        };
        _bubble.Token(BackgroundProperty, background).Token(BorderBrushProperty, ShellToken.Border);
        _bubble.BorderThickness = new Thickness(Role == ChatRole.System ? 1 : 0);
        _text.Text = Text ?? string.Empty;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        _text.FontStyle = Role == ChatRole.System ? FontStyle.Italic : FontStyle.Normal;
        _text.Token(TextBlock.ForegroundProperty, foreground);
        foreach (var dot in _dots.Children) ((Border)dot).Token(BackgroundProperty, foreground);

        _actions.Child = Actions;
        _actions.IsVisible = Actions != null;
        _dots.IsVisible = IsStreaming;
        UpdateDots();
    }

    // Each dot hops in turn, like animate-bounce with staggered delays; one cycle per second.
    private void UpdateDots()
    {
        if (!IsStreaming || !this.IsAttachedToVisualTree())
        {
            StopDots();
            return;
        }
        if (_timer != null) return;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
        {
            _phase = (_phase + 0.016) % 1;
            for (var i = 0; i < _dots.Children.Count; i++)
            {
                var t = (_phase - i * 0.15 + 1) % 1;
                var hop = t < 0.6 ? Math.Sin(t / 0.6 * Math.PI) : 0;
                ((TranslateTransform)_dots.Children[i].RenderTransform!).Y = -5 * hop;
            }
        });
        _timer.Start();
    }

    private void StopDots()
    {
        _timer?.Stop();
        _timer = null;
        foreach (var dot in _dots.Children) ((TranslateTransform)dot.RenderTransform!).Y = 0;
    }
}

// Message box — rounded-lg border border-input p-2: a text area that grows with its text (up to
// max-h-48) and a send button. Enter sends and Shift+Enter starts a new line; sending raises
// Submitted with the trimmed text and clears the box.
//   <ui:ChatInput Placeholder="Send a message..." Submitted="OnSend" />
public class ChatInput : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ChatInput, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<ChatInput, string?>(nameof(Placeholder), "Send a message...");

    // Shows a spinner in the send button and blocks sending, while a reply is on its way.
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ChatInput, bool>(nameof(IsLoading));

    // A control before the text (an attach button, say).
    public static readonly StyledProperty<Control?> ActionsProperty =
        AvaloniaProperty.Register<ChatInput, Control?>(nameof(Actions));

    private readonly TextBox _textBox;
    private readonly Button _send;
    private readonly Border _actions = new() { VerticalAlignment = VerticalAlignment.Bottom, IsVisible = false };

    static ChatInput()
    {
        TextProperty.Changed.AddClassHandler<ChatInput>((i, e) =>
        {
            if (i._textBox.Text != (string?)e.NewValue) i._textBox.Text = (string?)e.NewValue;
            i.UpdateSend();
        });
        PlaceholderProperty.Changed.AddClassHandler<ChatInput>((i, e) => i._textBox.PlaceholderText = (string?)e.NewValue);
        IsLoadingProperty.Changed.AddClassHandler<ChatInput>((i, _) => i.UpdateSend());
        IsEnabledProperty.Changed.AddClassHandler<ChatInput>((i, _) =>
        {
            i.Opacity = i.IsEnabled ? 1.0 : 0.5;
            i.UpdateSend();
        });
        ActionsProperty.Changed.AddClassHandler<ChatInput>((i, e) =>
        {
            i._actions.Child = e.NewValue as Control;
            i._actions.IsVisible = e.NewValue != null;
        });
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public Control? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    // Raised with the trimmed text.
    public event EventHandler<string>? Submitted;

    public ChatInput()
    {
        _textBox = new TextBox
        {
            FontSize = 14,
            PlaceholderText = Placeholder,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 192,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ShellPlatform.StripNativeChrome(_textBox);
        _textBox.MinHeight = 36; // after StripNativeChrome, which clears Fluent's minimum
        _textBox.TextChanged += (_, _) => Text = _textBox.Text;
        // Tunnel: with AcceptsReturn the text box would turn Enter into a new line.
        _textBox.AddHandler(KeyDownEvent, OnTextKeyDown, RoutingStrategies.Tunnel);
        _textBox.GotFocus += (_, _) => this.Token(BorderBrushProperty, ShellToken.Ring);
        _textBox.LostFocus += (_, _) => this.Token(BorderBrushProperty, ShellToken.Input);
        Grid.SetColumn(_textBox, 1);

        _send = new Button { Icon = IconName.ArrowRight, Size = ButtonSize.Icon, VerticalAlignment = VerticalAlignment.Bottom };
        AutomationProperties.SetName(_send, "Send");
        _send.Clicked += (_, _) => Submit();
        Grid.SetColumn(_send, 2);

        Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8, Children = { _actions, _textBox, _send } };
        Padding = new Thickness(8);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        Cursor = new Cursor(StandardCursorType.Ibeam);
        this.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Input);
        UpdateSend();
    }

    private bool CanSubmit => IsEffectivelyEnabled && !IsLoading && !string.IsNullOrWhiteSpace(Text);

    // Sends the text as the button or Enter does. Does nothing while loading or when the box is empty.
    public void Submit()
    {
        if (!CanSubmit) return;
        var text = Text!.Trim();
        Text = string.Empty;
        Submitted?.Invoke(this, text);
    }

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None)
        => _textBox.Focus(method, keyModifiers);

    // Clicking the padding focuses the text box too.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.Handled && !_textBox.IsFocused) _textBox.Focus();
    }

    private void OnTextKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        Submit();
        e.Handled = true;
    }

    private void UpdateSend()
    {
        _send.IsLoading = IsLoading;
        _send.IsEnabled = CanSubmit || IsLoading;
    }
}
