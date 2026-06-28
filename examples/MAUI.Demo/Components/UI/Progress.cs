using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Progress bar component
public partial class Progress : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Progress), 
            0.0, propertyChanged: OnValueChanged);

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Progress), 
            100.0, propertyChanged: OnValueChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ProgressVariant), typeof(Progress), 
            ProgressVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShowLabelProperty =
        BindableProperty.Create(nameof(ShowLabel), typeof(bool), typeof(Progress), 
            false, propertyChanged: OnShowLabelChanged);

    private readonly Border _track;
    private Border _fill;
    private readonly Label _label;
    private readonly Grid _container;

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Max(0, Math.Min(value, Maximum)));
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, Math.Max(1, value));
    }

    public ProgressVariant Variant
    {
        get => (ProgressVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    public double Percentage => Maximum > 0 ? (Value / Maximum) * 100 : 0;

    public Progress()
    {
        _fill = new Border
        {
            BackgroundColor = Color.FromArgb("#2563EB"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 9999 },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };

        _track = new Border
        {
            Content = _fill,
            BackgroundColor = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 9999 },
            HeightRequest = 8
        };

        _label = new Label
        {
            FontSize = 12,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(8, 0, 0, 0),
            IsVisible = false
        };

        _container = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        _container.Add(_track, 0, 0);
        _container.Add(_label, 1, 0);

        Content = _container;
        UpdateVisualState();
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Progress progress)
        {
            progress.UpdateProgress();
            progress.UpdateVisualState();
        }
    }

    private static void OnShowLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Progress progress)
            progress._label.IsVisible = (bool)newValue;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Progress progress)
            progress.UpdateVisualState();
    }

    private void UpdateProgress()
    {
        var percentage = Percentage;
        
        // Use Grid with proportional columns for progress fill
        if (_track.Content is not Grid fillGrid)
        {
            fillGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(percentage, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(100 - percentage, GridUnitType.Star) }
                }
            };

            var fillBar = new Border
            {
                BackgroundColor = _fill.BackgroundColor,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 9999 }
            };

            fillGrid.Add(fillBar, 0, 0);
            _track.Content = fillGrid;
            _fill = fillBar;
        }
        else
        {
            fillGrid.ColumnDefinitions[0].Width = new GridLength(percentage, GridUnitType.Star);
            fillGrid.ColumnDefinitions[1].Width = new GridLength(100 - percentage, GridUnitType.Star);
        }

        if (ShowLabel)
            _label.Text = $"{percentage:F0}%";
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme - variant colors
        var fillColor = Variant switch
        {
            ProgressVariant.Default => Color.FromArgb("#2563EB"),
            ProgressVariant.Success => Color.FromArgb("#22C55E"),
            ProgressVariant.Warning => Color.FromArgb("#F59E0B"),
            ProgressVariant.Destructive => Color.FromArgb("#EF4444"),
            _ => Color.FromArgb("#2563EB")
        };

        _track.BackgroundColor = Color.FromArgb("#E5E7EB");
        
        if (_fill != null)
            _fill.BackgroundColor = fillColor;

        _label.TextColor = Color.FromArgb("#6B7280");
        UpdateProgress();
    }
}

public enum ProgressVariant
{
    Default,
    Success,
    Warning,
    Destructive
}
