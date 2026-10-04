using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AlertDialogTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "alert-dialog",
        DisplayName = "Alert Dialog",
        Description = "Confirmation dialog that requires a choice",
        Category = ComponentCategory.Overlay,
        FilePath = "AlertDialog.cs",
        Dependencies = new List<string> { "shell", "element-extensions", "button" },
        Tags = new List<string> { "dialog", "confirm", "modal", "alert" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Confirmation dialog that requires a choice — no close button, and the backdrop doesn't
// dismiss it (shadcn AlertDialog). Place it where it can fill the page, like Dialog.
//   <ui:AlertDialog x:Name=""DeleteDialog"" Title=""Are you absolutely sure?""
//                   Description=""This action cannot be undone."" ConfirmText=""Delete""
//                   ConfirmVariant=""Destructive"" Confirmed=""OnDelete"" />
// From code: if (await DeleteDialog.ShowAsync()) { ... }
// Optional extra content (between description and buttons) goes inside the tag.
[ContentProperty(nameof(Body))]
public partial class AlertDialog : ShellOverlayHost
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AlertDialog), string.Empty,
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(AlertDialog), string.Empty,
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(nameof(ConfirmText), typeof(string), typeof(AlertDialog), ""Continue"",
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

    // Empty hides the cancel button.
    public static readonly BindableProperty CancelTextProperty =
        BindableProperty.Create(nameof(CancelText), typeof(string), typeof(AlertDialog), ""Cancel"",
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

    public static readonly BindableProperty ConfirmVariantProperty =
        BindableProperty.Create(nameof(ConfirmVariant), typeof(ButtonVariant), typeof(AlertDialog), ButtonVariant.Default,
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(AlertDialog), null,
            propertyChanged: (b, o, n) => ((AlertDialog)b)._panel.Update());

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

    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public ButtonVariant ConfirmVariant
    {
        get => (ButtonVariant)GetValue(ConfirmVariantProperty);
        set => SetValue(ConfirmVariantProperty, value);
    }

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    private readonly AlertDialogPanel _panel;
    private TaskCompletionSource<bool>? _pending;

    public AlertDialog()
    {
        _panel = new AlertDialogPanel(this);
        Children.Add(_panel);
    }

    protected override bool IsTrigger(Element child) => child is AlertDialogTrigger;

    // Escape / back counts as Cancel.
    protected override void Dismiss() => Resolve(false);

    // Opens the dialog and completes with true (confirm) or false (cancel).
    public Task<bool> ShowAsync()
    {
        _pending?.TrySetResult(false);
        _pending = new TaskCompletionSource<bool>();
        SetOpen(true);
        return _pending.Task;
    }

    internal void Resolve(bool confirmed)
    {
        SetOpen(false);
        if (confirmed) Confirmed?.Invoke(this, EventArgs.Empty);
        else Cancelled?.Invoke(this, EventArgs.Empty);
        _pending?.TrySetResult(confirmed);
        _pending = null;
    }
}

// Opens the enclosing AlertDialog. Usage: <ui:AlertDialogTrigger><ui:Button Text=""Delete"" /></ui:AlertDialogTrigger>
public partial class AlertDialogTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<AlertDialog>()?.SetOpen(true);
}

// Backdrop + centered box: max-w-lg rounded-lg border bg-background p-6 shadow-lg, fade + zoom-in-95.
internal sealed class AlertDialogPanel : ContentView, IShellOverlayContent
{
    private readonly AlertDialog _owner;
    private readonly BoxView _backdrop;
    private readonly Border _box;
    private readonly Label _title;
    private readonly Label _description;
    private readonly ContentView _body;
    private readonly Button _cancel;
    private readonly Button _confirm;

    public AlertDialogPanel(AlertDialog owner)
    {
        _owner = owner;
        _backdrop = new BoxView { BackgroundColor = Colors.Transparent };
        _backdrop.Token(BoxView.ColorProperty, ShellToken.Overlay);
        // Alert dialogs require a choice: swallow backdrop taps instead of closing.
        _backdrop.GestureRecognizers.Add(new TapGestureRecognizer());

        _title = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
        _title.Token(Label.TextColorProperty, ShellToken.Foreground);
        _description = new Label { FontSize = 14 };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _body = new ContentView();

        _cancel = new Button { Variant = ButtonVariant.Outline };
        _cancel.Clicked += (_, _) => _owner.Resolve(false);
        _confirm = new Button();
        _confirm.Clicked += (_, _) => _owner.Resolve(true);

        var footer = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { _cancel, _confirm }
        };

        _box = new Border
        {
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children = { _title, _description, _body, footer }
            },
            Padding = new Thickness(24),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 10), Radius = 24, Opacity = 0.2f }
        };
        _box.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _box.Token(Border.StrokeProperty, ShellToken.Border);
        _box.GestureRecognizers.Add(new TapGestureRecognizer());

        var root = new Grid { Children = { _backdrop, _box } };
        // The backdrop dims under the system bars; the centered box stays clear of them.
        ShellPortal.EdgeToEdge(this, root);
        root.SizeChanged += (_, _) => _box.WidthRequest = Math.Max(0, Math.Min(512, root.Width - 32));
        Content = root;
    }

    public void Update()
    {
        _title.Text = _owner.Title;
        _description.Text = _owner.Description;
        _description.IsVisible = !string.IsNullOrEmpty(_owner.Description);
        _body.Content = _owner.Body;
        _body.IsVisible = _owner.Body != null;
        _cancel.Text = _owner.CancelText;
        _cancel.IsVisible = !string.IsNullOrEmpty(_owner.CancelText);
        _confirm.Text = _owner.ConfirmText;
        _confirm.Variant = _owner.ConfirmVariant;
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            Update();
            _backdrop.Opacity = 0;
            _box.Opacity = 0;
            _box.Scale = 0.95;
            await Task.WhenAll(
                _backdrop.FadeToAsync(1, 150, Easing.CubicOut),
                _box.FadeToAsync(1, 150, Easing.CubicOut),
                _box.ScaleToAsync(1, 150, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(
                _backdrop.FadeToAsync(0, 120, Easing.CubicIn),
                _box.FadeToAsync(0, 120, Easing.CubicIn),
                _box.ScaleToAsync(0.95, 120, Easing.CubicIn));
        }
    }
}
"
    };
}
