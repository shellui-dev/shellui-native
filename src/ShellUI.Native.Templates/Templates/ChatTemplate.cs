using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ChatTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "chat",
        DisplayName = "Chat",
        Description = "Message list with bubbles, a typing indicator and a message box",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Chat.cs",
        Dependencies = new List<string> { "shell", "icon", "button" },
        Tags = new List<string> { "chat", "message", "conversation", "assistant" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Chat — a rounded-lg bordered panel: an optional header, a scrolling list of messages that
// follows the newest one, and an optional footer (usually a ChatInput). Give it a height.
//   <ui:Chat x:Name=""Conversation"" HeightRequest=""420"">
//       <ui:Chat.Header> ...any view... </ui:Chat.Header>
//       <ui:ChatMessage Role=""Assistant"" Sender=""ShellUI"" Text=""How can I help?"" AvatarText=""S"" />
//       <ui:ChatMessage Role=""User"" Text=""Show me the button."" />
//       <ui:Chat.Footer>
//           <ui:ChatInput Submitted=""OnSend"" />
//       </ui:Chat.Footer>
//   </ui:Chat>
// Add messages from code with Conversation.Messages.Add(new ChatMessage { ... }).
[ContentProperty(nameof(Messages))]
public partial class Chat : ContentView
{
    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(nameof(Header), typeof(View), typeof(Chat), null,
            propertyChanged: (b, o, n) => ((Chat)b).UpdateSlots());

    public static readonly BindableProperty FooterProperty =
        BindableProperty.Create(nameof(Footer), typeof(View), typeof(Chat), null,
            propertyChanged: (b, o, n) => ((Chat)b).UpdateSlots());

    // Scrolls to the newest message when one is added.
    public static readonly BindableProperty AutoScrollProperty =
        BindableProperty.Create(nameof(AutoScroll), typeof(bool), typeof(Chat), true);

    public View? Header
    {
        get => (View?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public View? Footer
    {
        get => (View?)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public IList<IView> Messages => _messages.Children;

    private readonly VerticalStackLayout _messages;
    private readonly ScrollView _scroll;
    private readonly ContentView _header;
    private readonly ContentView _footer;
    private readonly BoxView _headerLine;
    private readonly BoxView _footerLine;
    private double _lastHeight;

    public Chat()
    {
        _messages = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(16) };
        _messages.ChildAdded += (_, _) => { if (AutoScroll) ScrollToEnd(); };
        _scroll = new ScrollView { Content = _messages };
        // A message that grows (a reply still arriving) keeps the list at the end, unless the
        // reader has scrolled up.
        _messages.SizeChanged += (_, _) =>
        {
            var wasAtEnd = _scroll.ScrollY >= _lastHeight - _scroll.Height - 24;
            _lastHeight = _messages.Height;
            if (AutoScroll && wasAtEnd) ScrollToEnd(animated: false);
        };
        _header = new ContentView { Padding = new Thickness(16, 12), IsVisible = false };
        _footer = new ContentView { Padding = new Thickness(12), IsVisible = false };
        _headerLine = Line();
        _footerLine = Line();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)
            }
        };
        grid.Add(_header, 0, 0);
        grid.Add(_headerLine, 0, 1);
        grid.Add(_scroll, 0, 2);
        grid.Add(_footerLine, 0, 3);
        grid.Add(_footer, 0, 4);

        var border = new Border
        {
            Content = grid,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        border.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        border.Token(Border.StrokeProperty, ShellToken.Border);
        Content = border;
    }

    // Shows the newest message; waits a moment so a message added just now has its size.
    public async void ScrollToEnd(bool animated = true)
    {
        await Task.Delay(60);
        if (_scroll.Handler is null) return;
        await _scroll.ScrollToAsync(0, Math.Max(0, _messages.Height - _scroll.Height), animated);
    }

    private static BoxView Line()
    {
        var line = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent, IsVisible = false };
        line.Token(BoxView.ColorProperty, ShellToken.Border);
        return line;
    }

    private void UpdateSlots()
    {
        _header.Content = Header;
        _header.IsVisible = _headerLine.IsVisible = Header != null;
        _footer.Content = Footer;
        _footer.IsVisible = _footerLine.IsVisible = Footer != null;
    }
}

public enum ChatRole { Assistant, User, System }

// One message — an optional avatar beside a bubble (rounded-lg px-3 py-2 text-sm, at most 80%
// of the width). User messages sit on the right in bg-primary, assistant ones on the left in
// bg-muted, system ones are outlined and italic. Its child view goes inside the bubble.
[ContentProperty(nameof(Body))]
public partial class ChatMessage : ContentView
{
    public static readonly BindableProperty RoleProperty =
        BindableProperty.Create(nameof(Role), typeof(ChatRole), typeof(ChatMessage), ChatRole.Assistant,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ChatMessage), string.Empty,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    // The sender's name above the bubble.
    public static readonly BindableProperty SenderProperty =
        BindableProperty.Create(nameof(Sender), typeof(string), typeof(ChatMessage), string.Empty,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    public static readonly BindableProperty TimestampProperty =
        BindableProperty.Create(nameof(Timestamp), typeof(string), typeof(ChatMessage), string.Empty,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    // Shows three bouncing dots after the text while a reply is still arriving.
    public static readonly BindableProperty IsStreamingProperty =
        BindableProperty.Create(nameof(IsStreaming), typeof(bool), typeof(ChatMessage), false,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    // Initials shown in the avatar circle.
    public static readonly BindableProperty AvatarTextProperty =
        BindableProperty.Create(nameof(AvatarText), typeof(string), typeof(ChatMessage), string.Empty,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    // Any view for the avatar circle (an Avatar, an Icon); replaces AvatarText.
    public static readonly BindableProperty AvatarProperty =
        BindableProperty.Create(nameof(Avatar), typeof(View), typeof(ChatMessage), null,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    // A view inside the bubble, under the text.
    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(ChatMessage), null,
            propertyChanged: (b, o, n) => ((ChatMessage)b)._body.Content = n as View);

    // A view under the bubble (copy, retry, ...).
    public static readonly BindableProperty ActionsProperty =
        BindableProperty.Create(nameof(Actions), typeof(View), typeof(ChatMessage), null,
            propertyChanged: (b, o, n) => ((ChatMessage)b).Update());

    public ChatRole Role
    {
        get => (ChatRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Sender
    {
        get => (string)GetValue(SenderProperty);
        set => SetValue(SenderProperty, value);
    }

    public string Timestamp
    {
        get => (string)GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    public bool IsStreaming
    {
        get => (bool)GetValue(IsStreamingProperty);
        set => SetValue(IsStreamingProperty, value);
    }

    public string AvatarText
    {
        get => (string)GetValue(AvatarTextProperty);
        set => SetValue(AvatarTextProperty, value);
    }

    public View? Avatar
    {
        get => (View?)GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public View? Actions
    {
        get => (View?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    private const string DotsAnimation = ""ChatDots"";

    private readonly Grid _row;
    private readonly Border _avatar;
    private readonly Label _avatarText;
    private readonly VerticalStackLayout _column;
    private readonly HorizontalStackLayout _meta;
    private readonly Label _name;
    private readonly Label _timestamp;
    private readonly Border _bubble;
    private readonly Label _text;
    private readonly ContentView _body;
    private readonly HorizontalStackLayout _dots;
    private readonly ContentView _actions;

    public ChatMessage()
    {
        _avatarText = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        _avatarText.Token(Label.TextColorProperty, ShellToken.Foreground);
        // h-8 w-8 rounded-full bg-muted, clipped to the circle.
        _avatar = new Border
        {
            WidthRequest = 32,
            HeightRequest = 32,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            VerticalOptions = LayoutOptions.Start,
            IsVisible = false
        };
        _avatar.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);

        _name = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold };
        _name.Token(Label.TextColorProperty, ShellToken.Foreground);
        _timestamp = new Label { FontSize = 12 };
        _timestamp.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _meta = new HorizontalStackLayout { Spacing = 8, Children = { _name, _timestamp } };

        _text = new Label { FontSize = 14, LineHeight = 1.3, LineBreakMode = LineBreakMode.WordWrap };
        _body = new ContentView();
        _dots = new HorizontalStackLayout { Spacing = 3, HeightRequest = 14, IsVisible = false };
        for (var i = 0; i < 3; i++)
            _dots.Children.Add(new BoxView { WidthRequest = 6, HeightRequest = 6, CornerRadius = 3, VerticalOptions = LayoutOptions.End, BackgroundColor = Colors.Transparent });
        SemanticProperties.SetDescription(_dots, ""Typing"");

        _bubble = new Border
        {
            Content = new VerticalStackLayout { Spacing = 6, Children = { _text, _body, _dots } },
            Padding = new Thickness(12, 8),
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        _actions = new ContentView { IsVisible = false };
        _column = new VerticalStackLayout { Spacing = 4, Children = { _meta, _bubble, _actions } };

        _row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        _row.Add(_column, 1, 0);
        _row.Add(_avatar);
        // max-w-[80%]
        _row.SizeChanged += (_, _) => { if (_row.Width > 0) _column.MaximumWidthRequest = Math.Max(120, _row.Width * 0.8); };
        Content = _row;
        Update();
    }

    private void Update()
    {
        var user = Role == ChatRole.User;
        var side = user ? LayoutOptions.End : LayoutOptions.Start;

        _avatar.Content = Avatar ?? (string.IsNullOrEmpty(AvatarText) ? null : _avatarText);
        _avatarText.Text = AvatarText ?? string.Empty;
        _avatar.IsVisible = _avatar.Content != null;
        Grid.SetColumn(_avatar, user ? 2 : 0);

        _column.HorizontalOptions = side;
        _meta.HorizontalOptions = side;
        _bubble.HorizontalOptions = side;
        _actions.HorizontalOptions = side;

        _name.Text = Sender ?? string.Empty;
        _name.IsVisible = !string.IsNullOrEmpty(Sender);
        _timestamp.Text = Timestamp ?? string.Empty;
        _timestamp.IsVisible = !string.IsNullOrEmpty(Timestamp);
        _meta.IsVisible = _name.IsVisible || _timestamp.IsVisible;

        var (background, foreground) = Role switch
        {
            ChatRole.User => (ShellToken.Primary, ShellToken.PrimaryForeground),
            ChatRole.System => (ShellToken.Background, ShellToken.MutedForeground),
            _ => (ShellToken.Muted, ShellToken.Foreground)
        };
        _bubble.Token(VisualElement.BackgroundColorProperty, background);
        _bubble.StrokeThickness = Role == ChatRole.System ? 1 : 0;
        _bubble.Token(Border.StrokeProperty, ShellToken.Border);
        _text.Text = Text ?? string.Empty;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        _text.FontAttributes = Role == ChatRole.System ? FontAttributes.Italic : FontAttributes.None;
        _text.Token(Label.TextColorProperty, foreground);
        foreach (var dot in _dots.Children.OfType<BoxView>()) dot.Token(BoxView.ColorProperty, foreground);

        _actions.Content = Actions;
        _actions.IsVisible = Actions != null;

        _dots.IsVisible = IsStreaming;
        if (IsStreaming) StartDots(); else StopDots();
    }

    // Each dot hops in turn, like animate-bounce with staggered delays.
    private void StartDots()
    {
        if (this.AnimationIsRunning(DotsAnimation)) return;
        var dots = _dots.Children.OfType<BoxView>().ToList();
        var animation = new Animation();
        for (var i = 0; i < dots.Count; i++)
        {
            var dot = dots[i];
            var start = i * 0.15;
            animation.Add(start, start + 0.3, new Animation(v => dot.TranslationY = v, 0, -5, Easing.CubicOut));
            animation.Add(start + 0.3, start + 0.6, new Animation(v => dot.TranslationY = v, -5, 0, Easing.CubicIn));
        }
        animation.Commit(this, DotsAnimation, 16, 1000, Easing.Linear, repeat: () => IsStreaming);
    }

    private void StopDots()
    {
        this.AbortAnimation(DotsAnimation);
        foreach (var dot in _dots.Children.OfType<BoxView>()) dot.TranslationY = 0;
    }
}

// Message box — rounded-lg border border-input p-2: a text area that grows with its text (up to
// max-h-48) and a send button. The button, or Submit(), raises Submitted with the trimmed text
// and clears the box.
//   <ui:ChatInput Placeholder=""Send a message..."" Submitted=""OnSend"" />
public partial class ChatInput : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ChatInput), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) =>
            {
                var input = (ChatInput)b;
                if (input._editor.Text != n as string) input._editor.Text = n as string ?? string.Empty;
                input.UpdateSend();
            });

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ChatInput), ""Send a message..."",
            propertyChanged: (b, o, n) => ((ChatInput)b)._editor.Placeholder = n as string ?? string.Empty);

    // Shows a spinner in the send button and blocks sending, while a reply is on its way.
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(ChatInput), false,
            propertyChanged: (b, o, n) => ((ChatInput)b).UpdateSend());

    // A view before the text (an attach button, say).
    public static readonly BindableProperty ActionsProperty =
        BindableProperty.Create(nameof(Actions), typeof(View), typeof(ChatInput), null,
            propertyChanged: (b, o, n) =>
            {
                var input = (ChatInput)b;
                input._actions.Content = n as View;
                input._actions.IsVisible = n != null;
            });

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public View? Actions
    {
        get => (View?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    // Raised with the trimmed text.
    public event EventHandler<string>? Submitted;

    private readonly Border _border;
    private readonly Editor _editor;
    private readonly Button _send;
    private readonly ContentView _actions;

    public ChatInput()
    {
        _editor = new Editor
        {
            FontSize = 14,
            Placeholder = Placeholder,
            BackgroundColor = Colors.Transparent,
            MinimumHeightRequest = 36,
            MaximumHeightRequest = 192,
            AutoSize = EditorAutoSizeOption.TextChanges,
            VerticalOptions = LayoutOptions.Center
        };
        _editor.Token(Editor.TextColorProperty, ShellToken.Foreground);
        _editor.Token(Editor.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_editor);
        ShellFocus.Track(_editor);
        _editor.TextChanged += (_, e) => Text = e.NewTextValue;
        _editor.Focused += (_, _) => _border!.Token(Border.StrokeProperty, ShellToken.Ring);
        _editor.Unfocused += (_, _) => _border!.Token(Border.StrokeProperty, ShellToken.Input);

        _send = new Button { Icon = IconName.ArrowRight, Size = ButtonSize.Icon, VerticalOptions = LayoutOptions.End };
        SemanticProperties.SetDescription(_send, ""Send"");
        _send.Clicked += (_, _) => Submit();
        _actions = new ContentView { VerticalOptions = LayoutOptions.End, IsVisible = false };

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        row.Add(_actions, 0, 0);
        row.Add(_editor, 1, 0);
        row.Add(_send, 2, 0);

        _border = new Border
        {
            Content = row,
            Padding = new Thickness(8),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _border.Token(Border.StrokeProperty, ShellToken.Input);
        // Tapping the padding focuses the editor too.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _editor.Focus();
        _border.GestureRecognizers.Add(tap);

        Content = _border;
        UpdateSend();
    }

    private bool CanSubmit => IsEnabled && !IsLoading && !string.IsNullOrWhiteSpace(Text);

    // Sends the text as the button does. Does nothing while loading or when the box is empty.
    public void Submit()
    {
        if (!CanSubmit) return;
        var text = Text.Trim();
        Text = string.Empty;
        Submitted?.Invoke(this, text);
    }

    public new void Focus() => _editor.Focus();

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _send != null)
        {
            Opacity = IsEnabled ? 1.0 : 0.5;
            UpdateSend();
        }
    }

    private void UpdateSend()
    {
        _send.IsLoading = IsLoading;
        _send.IsEnabled = CanSubmit || IsLoading;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
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
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Chat — a rounded-lg bordered panel: an optional header, a scrolling list of messages that
// follows the newest one, and an optional footer (usually a ChatInput). Give it a height.
//   <ui:Chat x:Name=""Conversation"" Height=""420"">
//       <ui:Chat.Header> ...any control... </ui:Chat.Header>
//       <ui:ChatMessage Role=""Assistant"" Sender=""ShellUI"" Text=""How can I help?"" AvatarText=""S"" />
//       <ui:ChatMessage Role=""User"" Text=""Show me the button."" />
//       <ui:Chat.Footer>
//           <ui:ChatInput Submitted=""OnSend"" />
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

        Child = new Grid { RowDefinitions = new RowDefinitions(""Auto,*,Auto""), Children = { _header, _scroll, _footer } };
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
        AutomationProperties.SetName(_dots, ""Typing"");

        _bubble = new Border
        {
            Child = new StackPanel { Spacing = 6, Children = { _text, _body, _dots } },
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(ShellTheme.RadiusLg)
        };
        _column = new StackPanel { Spacing = 4, Children = { _meta, _bubble, _actions } };
        Grid.SetColumn(_column, 1);

        _row = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""), ColumnSpacing = 12, Children = { _column, _avatar } };
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
//   <ui:ChatInput Placeholder=""Send a message..."" Submitted=""OnSend"" />
public class ChatInput : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ChatInput, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<ChatInput, string?>(nameof(Placeholder), ""Send a message..."");

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
        AutomationProperties.SetName(_send, ""Send"");
        _send.Clicked += (_, _) => Submit();
        Grid.SetColumn(_send, 2);

        Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""), ColumnSpacing = 8, Children = { _actions, _textBox, _send } };
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
"
    };
}
