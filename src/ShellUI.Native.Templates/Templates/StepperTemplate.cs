using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class StepperTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "stepper",
        DisplayName = "Stepper",
        Description = "Step-by-step flow with numbered steps and navigation",
        Category = ComponentCategory.Navigation,
        FilePath = "Stepper.cs",
        Dependencies = new List<string> { "shell", "icon", "button" },
        Tags = new List<string> { "stepper", "wizard", "steps", "progress" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Step-by-step flow — numbered circles joined by lines (active filled, completed checked), the
// active step's content, and Previous / Next / Confirm buttons. A step becomes clickable once it
// has been reached.
//   <ui:Stepper Confirmed=""OnDone"">
//       <ui:StepperStep Title=""Account"" Description=""Your details""> ...any view... </ui:StepperStep>
//       <ui:StepperStep Title=""Plan""> ... </ui:StepperStep>
//       <ui:StepperStep Title=""Review""> ... </ui:StepperStep>
//   </ui:Stepper>
[ContentProperty(nameof(Steps))]
public partial class Stepper : ContentView
{
    public static readonly BindableProperty CurrentStepProperty =
        BindableProperty.Create(nameof(CurrentStep), typeof(int), typeof(Stepper), 0, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Stepper)b).OnCurrentStepChanged());

    public static readonly BindableProperty ShowNavigationProperty =
        BindableProperty.Create(nameof(ShowNavigation), typeof(bool), typeof(Stepper), true,
            propertyChanged: (b, o, n) => ((Stepper)b).UpdateState());

    public static readonly BindableProperty PreviousTextProperty =
        BindableProperty.Create(nameof(PreviousText), typeof(string), typeof(Stepper), ""Previous"",
            propertyChanged: (b, o, n) => ((Stepper)b).UpdateState());

    public static readonly BindableProperty NextTextProperty =
        BindableProperty.Create(nameof(NextText), typeof(string), typeof(Stepper), ""Next"",
            propertyChanged: (b, o, n) => ((Stepper)b).UpdateState());

    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(nameof(ConfirmText), typeof(string), typeof(Stepper), ""Confirm"",
            propertyChanged: (b, o, n) => ((Stepper)b).UpdateState());

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public bool ShowNavigation
    {
        get => (bool)GetValue(ShowNavigationProperty);
        set => SetValue(ShowNavigationProperty, value);
    }

    public string PreviousText
    {
        get => (string)GetValue(PreviousTextProperty);
        set => SetValue(PreviousTextProperty, value);
    }

    public string NextText
    {
        get => (string)GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    public IList<StepperStep> Steps => _steps;

    // User input only: setting CurrentStep, or calling Next / Previous, from code doesn't raise it.
    public event EventHandler<int>? CurrentStepChanged;
    // Raised by the Confirm button on the last step.
    public event EventHandler? Confirmed;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    private const double CircleSize = 32;

    private sealed class Marker(Border circle, Label number, Icon check, Label title, BoxView before, BoxView after)
    {
        public Border Circle { get; } = circle;
        public Label Number { get; } = number;
        public Icon Check { get; } = check;
        public Label Title { get; } = title;
        public BoxView Before { get; } = before;
        public BoxView After { get; } = after;
    }

    private readonly ObservableCollection<StepperStep> _steps = new();
    private readonly List<Marker> _markers = new();
    private readonly Grid _header;
    private readonly ContentView _body;
    private readonly Grid _navigation;
    private readonly Button _previous;
    private readonly Button _next;
    private int _highest;

    public Stepper()
    {
        _header = new Grid();
        _body = new ContentView { Margin = new Thickness(0, 24, 0, 0) };

        _previous = new Button { Variant = ButtonVariant.Outline, HorizontalOptions = LayoutOptions.Start };
        _previous.Clicked += (_, _) => ByUser(Previous);
        _next = new Button { HorizontalOptions = LayoutOptions.End };
        _next.Clicked += (_, _) =>
        {
            if (IsLast)
            {
                _highest = Math.Max(_highest, _steps.Count - 1);
                Confirmed?.Invoke(this, EventArgs.Empty);
            }
            else ByUser(Next);
        };
        _navigation = new Grid { Margin = new Thickness(0, 24, 0, 0), Children = { _previous, _next } };

        Content = new VerticalStackLayout { Spacing = 0, Children = { _header, _body, _navigation } };
        _steps.CollectionChanged += (_, _) => Rebuild();
        Rebuild();
    }

    private int Current => Math.Clamp(CurrentStep, 0, Math.Max(0, _steps.Count - 1));
    private bool IsLast => Current >= _steps.Count - 1;

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void Next()
    {
        if (!IsLast) CurrentStep = Current + 1;
    }

    public void Previous()
    {
        if (Current > 0) CurrentStep = Current - 1;
    }

    private void OnCurrentStepChanged()
    {
        _highest = Math.Max(_highest, Current);
        UpdateState();
        if (_byUser) CurrentStepChanged?.Invoke(this, Current);
    }

    private void Rebuild()
    {
        _header.Children.Clear();
        _header.ColumnDefinitions.Clear();
        _markers.Clear();

        for (var i = 0; i < _steps.Count; i++)
        {
            var index = i;
            var step = _steps[i];
            step.PropertyChanged -= OnStepPropertyChanged;
            step.PropertyChanged += OnStepPropertyChanged;

            var number = new Label
            {
                Text = (i + 1).ToString(),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            var check = new Icon { Name = IconName.Check, Size = 16, Token = ShellToken.Primary, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            var circle = new Border
            {
                Content = new Grid { Children = { number, check } },
                WidthRequest = CircleSize,
                HeightRequest = CircleSize,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = CircleSize / 2 }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => { if (index <= _highest) ByUser(() => CurrentStep = index); };
            circle.GestureRecognizers.Add(tap);

            // Lines run from each circle to the edges of its column, so neighbors meet.
            var before = Line(i > 0);
            var after = Line(i < _steps.Count - 1);
            var rail = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
            };
            rail.Add(before, 0, 0);
            rail.Add(circle, 1, 0);
            rail.Add(after, 2, 0);

            var title = new Label
            {
                Text = step.Title,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(4, 8, 4, 0),
                IsVisible = !string.IsNullOrEmpty(step.Title)
            };
            var description = new Label
            {
                Text = step.Description,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(4, 2, 4, 0),
                IsVisible = !string.IsNullOrEmpty(step.Description)
            };
            description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

            _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _header.Add(new VerticalStackLayout { Spacing = 0, Children = { rail, title, description } }, i, 0);
            _markers.Add(new Marker(circle, number, check, title, before, after));
        }

        _highest = Math.Min(_highest, Math.Max(0, _steps.Count - 1));
        UpdateState();

        static BoxView Line(bool visible) => new()
        {
            HeightRequest = 2,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Opacity = visible ? 1 : 0
        };
    }

    private void OnStepPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StepperStep.Title) or nameof(StepperStep.Description)) Rebuild();
    }

    private void UpdateState()
    {
        var current = Current;
        for (var i = 0; i < _markers.Count; i++)
        {
            var marker = _markers[i];
            var active = i == current;
            var completed = i < _highest && !active;

            if (active) marker.Circle.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);
            else
            {
                marker.Circle.ClearValue(VisualElement.BackgroundColorProperty);
                marker.Circle.BackgroundColor = Colors.Transparent;
            }
            marker.Circle.Token(Border.StrokeProperty, active || completed ? ShellToken.Primary : ShellToken.Border);
            marker.Number.IsVisible = !completed;
            marker.Number.Token(Label.TextColorProperty, active ? ShellToken.PrimaryForeground : ShellToken.MutedForeground);
            marker.Check.IsVisible = completed;
            marker.Title.Token(Label.TextColorProperty, active || completed ? ShellToken.Foreground : ShellToken.MutedForeground);
            marker.Before.Token(BoxView.ColorProperty, i <= _highest ? ShellToken.Primary : ShellToken.Border);
            marker.After.Token(BoxView.ColorProperty, i < _highest ? ShellToken.Primary : ShellToken.Border);
        }

        _body.Content = _steps.Count > 0 ? _steps[current] : null;

        _navigation.IsVisible = ShowNavigation && _steps.Count > 0;
        _previous.Text = PreviousText;
        _previous.IsEnabled = current > 0;
        _next.Text = IsLast ? ConfirmText : NextText;
    }
}

// One step of a Stepper: its Title and Description label the step, its content shows while the
// step is active.
public partial class StepperStep : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(StepperStep), string.Empty);

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(StepperStep), string.Empty);

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
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Step-by-step flow — numbered circles joined by lines (active filled, completed checked), the
// active step's content, and Previous / Next / Confirm buttons. A step becomes clickable (and a
// tab stop) once it has been reached.
//   <ui:Stepper Confirmed=""OnDone"">
//       <ui:StepperStep Title=""Account"" Description=""Your details""> ...any control... </ui:StepperStep>
//       <ui:StepperStep Title=""Plan""> ... </ui:StepperStep>
//       <ui:StepperStep Title=""Review""> ... </ui:StepperStep>
//   </ui:Stepper>
public class Stepper : Border
{
    public static readonly StyledProperty<int> CurrentStepProperty =
        AvaloniaProperty.Register<Stepper, int>(nameof(CurrentStep), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> ShowNavigationProperty =
        AvaloniaProperty.Register<Stepper, bool>(nameof(ShowNavigation), true);

    public static readonly StyledProperty<string> PreviousTextProperty =
        AvaloniaProperty.Register<Stepper, string>(nameof(PreviousText), ""Previous"");

    public static readonly StyledProperty<string> NextTextProperty =
        AvaloniaProperty.Register<Stepper, string>(nameof(NextText), ""Next"");

    public static readonly StyledProperty<string> ConfirmTextProperty =
        AvaloniaProperty.Register<Stepper, string>(nameof(ConfirmText), ""Confirm"");

    private const double CircleSize = 32;

    private sealed class Marker
    {
        public Marker(Border circle, TextBlock number, Icon check, TextBlock title, Border before, Border after)
        {
            Circle = circle;
            Number = number;
            Check = check;
            Title = title;
            Before = before;
            After = after;
        }

        public Border Circle { get; }
        public TextBlock Number { get; }
        public Icon Check { get; }
        public TextBlock Title { get; }
        public Border Before { get; }
        public Border After { get; }
    }

    private readonly List<Marker> _markers = new();
    private readonly Grid _header = new();
    private readonly Panel _body = new() { Margin = new Thickness(0, 24, 0, 0) };
    private readonly Panel _navigation;
    private readonly Button _previous;
    private readonly Button _next;
    private int _highest;

    static Stepper()
    {
        CurrentStepProperty.Changed.AddClassHandler<Stepper>((s, _) => s.OnCurrentStepChanged());
        ShowNavigationProperty.Changed.AddClassHandler<Stepper>((s, _) => s.UpdateState());
        PreviousTextProperty.Changed.AddClassHandler<Stepper>((s, _) => s.UpdateState());
        NextTextProperty.Changed.AddClassHandler<Stepper>((s, _) => s.UpdateState());
        ConfirmTextProperty.Changed.AddClassHandler<Stepper>((s, _) => s.UpdateState());
    }

    public int CurrentStep
    {
        get => GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public bool ShowNavigation
    {
        get => GetValue(ShowNavigationProperty);
        set => SetValue(ShowNavigationProperty, value);
    }

    public string PreviousText
    {
        get => GetValue(PreviousTextProperty);
        set => SetValue(PreviousTextProperty, value);
    }

    public string NextText
    {
        get => GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public string ConfirmText
    {
        get => GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    // The StepperStep children. Every step stays in the tree; only the active one is visible.
    [Content]
    public Controls Steps => _body.Children;

    // User input only: setting CurrentStep, or calling Next / Previous, from code doesn't raise it.
    public event EventHandler<int>? CurrentStepChanged;
    // Raised by the Confirm button on the last step.
    public event EventHandler? Confirmed;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public Stepper()
    {
        _previous = new Button { Variant = ButtonVariant.Outline, HorizontalAlignment = HorizontalAlignment.Left };
        _previous.Clicked += (_, _) => ByUser(Previous);
        _next = new Button { HorizontalAlignment = HorizontalAlignment.Right };
        _next.Clicked += (_, _) =>
        {
            if (IsLast)
            {
                _highest = Math.Max(_highest, StepList.Count - 1);
                Confirmed?.Invoke(this, EventArgs.Empty);
            }
            else ByUser(Next);
        };
        _navigation = new Panel { Margin = new Thickness(0, 24, 0, 0), Children = { _previous, _next } };

        Child = new StackPanel { Children = { _header, _body, _navigation } };
        _body.Children.CollectionChanged += (_, _) => Rebuild();
        Rebuild();
    }

    private List<StepperStep> StepList => _body.Children.OfType<StepperStep>().ToList();
    private int Current => Math.Clamp(CurrentStep, 0, Math.Max(0, StepList.Count - 1));
    private bool IsLast => Current >= StepList.Count - 1;

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void Next()
    {
        if (!IsLast) CurrentStep = Current + 1;
    }

    public void Previous()
    {
        if (Current > 0) CurrentStep = Current - 1;
    }

    private void OnCurrentStepChanged()
    {
        _highest = Math.Max(_highest, Current);
        UpdateState();
        if (_byUser) CurrentStepChanged?.Invoke(this, Current);
    }

    private void Rebuild()
    {
        var steps = StepList;
        _header.Children.Clear();
        _header.ColumnDefinitions.Clear();
        _markers.Clear();

        for (var i = 0; i < steps.Count; i++)
        {
            var index = i;
            var step = steps[i];
            step.PropertyChanged -= OnStepPropertyChanged;
            step.PropertyChanged += OnStepPropertyChanged;

            var number = new TextBlock
            {
                Text = (i + 1).ToString(CultureInfo.CurrentCulture),
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var check = new Icon { Kind = IconName.Check, Size = 16, Token = ShellToken.Primary, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var circle = new Border
            {
                Child = new Panel { Children = { number, check } },
                Width = CircleSize,
                Height = CircleSize,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(CircleSize / 2)
            };
            AutomationProperties.SetName(circle, string.IsNullOrEmpty(step.Title) ? $""Step {i + 1}"" : step.Title);
            ShellFocus.Ring(circle, circle);
            circle.PointerReleased += (_, e) =>
            {
                if (e.InitialPressMouseButton != MouseButton.Left || index > _highest) return;
                ByUser(() => CurrentStep = index);
                e.Handled = true;
            };
            circle.KeyDown += (_, e) =>
            {
                if (e.Key is not (Key.Enter or Key.Space) || index > _highest) return;
                ByUser(() => CurrentStep = index);
                e.Handled = true;
            };

            // Lines run from each circle to the edges of its column, so neighbors meet.
            var before = Line(i > 0);
            var after = Line(i < steps.Count - 1);
            var rail = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto,*"") };
            Grid.SetColumn(circle, 1);
            Grid.SetColumn(after, 2);
            rail.Children.Add(before);
            rail.Children.Add(circle);
            rail.Children.Add(after);

            var title = new TextBlock
            {
                Text = step.Title,
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(4, 8, 4, 0),
                IsVisible = !string.IsNullOrEmpty(step.Title)
            };
            var description = new TextBlock
            {
                Text = step.Description,
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(4, 2, 4, 0),
                IsVisible = !string.IsNullOrEmpty(step.Description)
            };
            description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

            var column = new StackPanel { Children = { rail, title, description } };
            Grid.SetColumn(column, i);
            _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _header.Children.Add(column);
            _markers.Add(new Marker(circle, number, check, title, before, after));
        }

        _highest = Math.Min(_highest, Math.Max(0, steps.Count - 1));
        UpdateState();

        static Border Line(bool visible) => new()
        {
            Height = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = visible ? 1 : 0
        };
    }

    private void OnStepPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StepperStep.TitleProperty || e.Property == StepperStep.DescriptionProperty) Rebuild();
    }

    private void UpdateState()
    {
        var steps = StepList;
        var current = Current;
        for (var i = 0; i < _markers.Count; i++)
        {
            var marker = _markers[i];
            var active = i == current;
            var completed = i < _highest && !active;
            var reached = i <= _highest;

            if (active) marker.Circle.Token(BackgroundProperty, ShellToken.Primary);
            else
            {
                // Transparent, not null, so the whole circle stays hit-testable.
                marker.Circle.ClearToken(BackgroundProperty);
                marker.Circle.Background = Brushes.Transparent;
            }
            marker.Circle.Token(BorderBrushProperty, active || completed ? ShellToken.Primary : ShellToken.Border);
            marker.Circle.Focusable = reached;
            marker.Circle.Cursor = reached ? new Cursor(StandardCursorType.Hand) : null;
            marker.Number.IsVisible = !completed;
            marker.Number.Token(TextBlock.ForegroundProperty, active ? ShellToken.PrimaryForeground : ShellToken.MutedForeground);
            marker.Check.IsVisible = completed;
            marker.Title.Token(TextBlock.ForegroundProperty, active || completed ? ShellToken.Foreground : ShellToken.MutedForeground);
            marker.Before.Token(BackgroundProperty, reached ? ShellToken.Primary : ShellToken.Border);
            marker.After.Token(BackgroundProperty, i < _highest ? ShellToken.Primary : ShellToken.Border);
        }

        for (var i = 0; i < steps.Count; i++)
            steps[i].IsVisible = i == current;

        _navigation.IsVisible = ShowNavigation && steps.Count > 0;
        _previous.Text = PreviousText;
        _previous.IsEnabled = current > 0;
        _next.Text = IsLast ? ConfirmText : NextText;
    }
}

// One step of a Stepper: its Title and Description label the step, its content shows while the
// step is active.
public class StepperStep : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StepperStep, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StepperStep, string?>(nameof(Description));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
"
    };
}
