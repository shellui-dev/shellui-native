using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SpinnerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "spinner",
        DisplayName = "Spinner",
        Description = "Rotating loading indicator",
        Category = ComponentCategory.Feedback,
        FilePath = "Spinner.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "loading", "spinner", "progress" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Loading spinner — a rotating loader-circle (ShellUI Loading, spinner variant). Spins only while
// on screen and IsRunning. Usage: <ui:Spinner />  <ui:Spinner Size=""Lg"" Token=""MutedForeground"" />
public partial class Spinner : ContentView
{
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(SpinnerSize), typeof(Spinner), SpinnerSize.Default,
            propertyChanged: (b, o, n) => ((Spinner)b).UpdateSize());

    public static readonly BindableProperty TokenProperty =
        BindableProperty.Create(nameof(Token), typeof(ShellToken), typeof(Spinner), ShellToken.Foreground,
            propertyChanged: (b, o, n) => ((Spinner)b)._icon.Token = (ShellToken)n);

    public static readonly BindableProperty IsRunningProperty =
        BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(Spinner), true,
            propertyChanged: (b, o, n) => ((Spinner)b).UpdateRunning());

    public SpinnerSize Size
    {
        get => (SpinnerSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public ShellToken Token
    {
        get => (ShellToken)GetValue(TokenProperty);
        set => SetValue(TokenProperty, value);
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    private readonly Icon _icon;

    public Spinner()
    {
        _icon = new Icon { Name = IconName.LoaderCircle };
        Content = _icon;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        SemanticProperties.SetDescription(this, ""Loading"");
        UpdateSize();
        Loaded += (_, _) => UpdateRunning();
        Unloaded += (_, _) => this.AbortAnimation(""Spin"");
    }

    private void UpdateSize() => _icon.Size = Size switch
    {
        SpinnerSize.Sm => 16,
        SpinnerSize.Lg => 32,
        _ => 24
    };

    private void UpdateRunning()
    {
        IsVisible = IsRunning;
        if (!IsRunning || !IsLoaded)
        {
            this.AbortAnimation(""Spin"");
            return;
        }
        if (this.AnimationIsRunning(""Spin"")) return;
        new Animation(v => _icon.Rotation = v, 0, 360)
            .Commit(this, ""Spin"", 16, 800, Easing.Linear, repeat: () => IsRunning);
    }
}

public enum SpinnerSize { Sm, Default, Lg }
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace YourProjectNamespace.Components.UI;

// Loading spinner — a rotating loader-circle (ShellUI Loading, spinner variant). Spins only while
// on screen and IsRunning. Usage: <ui:Spinner />  <ui:Spinner Size=""Lg"" Token=""MutedForeground"" />
public class Spinner : Border
{
    public static readonly StyledProperty<SpinnerSize> SizeProperty =
        AvaloniaProperty.Register<Spinner, SpinnerSize>(nameof(Size), SpinnerSize.Default);

    public static readonly StyledProperty<ShellToken> TokenProperty =
        AvaloniaProperty.Register<Spinner, ShellToken>(nameof(Token), ShellToken.Foreground);

    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<Spinner, bool>(nameof(IsRunning), true);

    private readonly Icon _icon;
    private readonly RotateTransform _spin = new();
    private DispatcherTimer? _timer;

    static Spinner()
    {
        SizeProperty.Changed.AddClassHandler<Spinner>((s, _) => s.UpdateSize());
        TokenProperty.Changed.AddClassHandler<Spinner>((s, _) => s._icon.Token = s.Token);
        IsRunningProperty.Changed.AddClassHandler<Spinner>((s, _) => s.UpdateRunning());
    }

    public SpinnerSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public ShellToken Token
    {
        get => GetValue(TokenProperty);
        set => SetValue(TokenProperty, value);
    }

    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public Spinner()
    {
        _icon = new Icon { Kind = IconName.LoaderCircle, Token = ShellToken.Foreground, RenderTransform = _spin };
        Child = _icon;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(this, ""Loading"");
        UpdateSize();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateRunning();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Stop();
    }

    private void UpdateSize() => _icon.Size = Size switch
    {
        SpinnerSize.Sm => 16,
        SpinnerSize.Lg => 32,
        _ => 24
    };

    // One turn per 800 ms, advanced once per frame (as Button's loading state).
    private void UpdateRunning()
    {
        IsVisible = IsRunning;
        if (!IsRunning || TopLevel.GetTopLevel(this) is null)
        {
            Stop();
            return;
        }
        if (_timer != null) return;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render,
            (_, _) => _spin.Angle = (_spin.Angle + 360 * 16 / 800.0) % 360);
        _timer.Start();
    }

    private void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }
}

public enum SpinnerSize { Sm, Default, Lg }
"
    };
}
