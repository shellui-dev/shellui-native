namespace MAUI.Demo.Components.UI;

// Form — fields stacked with space-y-6. Each FormField is a label, a control, an optional
// description and an error message; validation is yours: set a field's Error and the form
// reports it.
//   <ui:Form x:Name="SignUp">
//       <ui:FormField x:Name="EmailField" Label="Email" Description="We never share it." IsRequired="True">
//           <ui:Input x:Name="Email" Placeholder="you@example.com" />
//       </ui:FormField>
//       <ui:Button Text="Create account" Clicked="OnSubmit" />
//   </ui:Form>
//   EmailField.Error = Email.Text.Contains('@') ? "" : "Enter a valid email.";
//   if (SignUp.IsValid) ...
[ContentProperty(nameof(Fields))]
public partial class Form : ContentView
{
    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(nameof(Spacing), typeof(double), typeof(Form), 24d,
            propertyChanged: (b, o, n) => ((Form)b)._stack.Spacing = (double)n);

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    // FormFields and anything else the form holds (buttons, separators).
    public IList<IView> Fields => _stack.Children;

    // True when no field in the form has an Error.
    public bool IsValid => FormFields().All(f => string.IsNullOrEmpty(f.Error));

    // The first field with an Error, top to bottom.
    public FormField? FirstInvalid => FormFields().FirstOrDefault(f => !string.IsNullOrEmpty(f.Error));

    private readonly VerticalStackLayout _stack;

    public Form()
    {
        _stack = new VerticalStackLayout { Spacing = 24 };
        Content = _stack;
    }

    // Every FormField in the form, however deeply it is nested.
    public IEnumerable<FormField> FormFields() => Walk(_stack);

    public void ClearErrors()
    {
        foreach (var field in FormFields()) field.Error = string.Empty;
    }

    private static IEnumerable<FormField> Walk(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is FormField field) yield return field;
            else
                foreach (var nested in Walk(child)) yield return nested;
        }
    }
}

// One field of a Form — space-y-2: a text-sm font-medium label, the control, a muted
// description and, when Error is set, the error in text-destructive (the label turns red too).
[ContentProperty(nameof(Control))]
public partial class FormField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, o, n) => ((FormField)b).Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, o, n) => ((FormField)b).Update());

    // The validation message; empty when the field is valid.
    public static readonly BindableProperty ErrorProperty =
        BindableProperty.Create(nameof(Error), typeof(string), typeof(FormField), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((FormField)b).Update());

    // Adds a red asterisk after the label.
    public static readonly BindableProperty IsRequiredProperty =
        BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(FormField), false,
            propertyChanged: (b, o, n) => ((FormField)b).Update());

    public static readonly BindableProperty ControlProperty =
        BindableProperty.Create(nameof(Control), typeof(View), typeof(FormField), null,
            propertyChanged: (b, o, n) => ((FormField)b)._control.Content = n as View);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Error
    {
        get => (string)GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public View? Control
    {
        get => (View?)GetValue(ControlProperty);
        set => SetValue(ControlProperty, value);
    }

    public bool HasError => !string.IsNullOrEmpty(Error);

    private readonly Microsoft.Maui.Controls.Label _label;
    private readonly Microsoft.Maui.Controls.Label _required;
    private readonly HorizontalStackLayout _labelRow;
    private readonly ContentView _control;
    private readonly Microsoft.Maui.Controls.Label _description;
    private readonly Microsoft.Maui.Controls.Label _error;

    public FormField()
    {
        _label = new Microsoft.Maui.Controls.Label { FontSize = 14, FontAttributes = FontAttributes.Bold };
        _required = new Microsoft.Maui.Controls.Label { Text = "*", FontSize = 14, FontAttributes = FontAttributes.Bold, IsVisible = false };
        _required.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.Destructive);
        _labelRow = new HorizontalStackLayout { Spacing = 4, Children = { _label, _required } };
        _control = new ContentView();
        _description = new Microsoft.Maui.Controls.Label { FontSize = 14, IsVisible = false };
        _description.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.MutedForeground);
        _error = new Microsoft.Maui.Controls.Label { FontSize = 14, FontAttributes = FontAttributes.Bold, IsVisible = false };
        _error.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.Destructive);

        Content = new VerticalStackLayout { Spacing = 8, Children = { _labelRow, _control, _description, _error } };
        Update();
    }

    private void Update()
    {
        _label.Text = Label ?? string.Empty;
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, HasError ? ShellToken.Destructive : ShellToken.Foreground);
        _required.IsVisible = IsRequired;
        _labelRow.IsVisible = !string.IsNullOrEmpty(Label);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _error.Text = Error ?? string.Empty;
        _error.IsVisible = HasError;
        // Screen readers announce the message when it appears.
        SemanticProperties.SetHint(this, Error ?? string.Empty);
    }
}
