using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class FormTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "form",
        DisplayName = "Form",
        Description = "Fields with a label, description and error message",
        Category = ComponentCategory.Form,
        FilePath = "Form.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "form", "field", "validation", "label" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Form — fields stacked with space-y-6. Each FormField is a label, a control, an optional
// description and an error message; validation is yours: set a field's Error and the form
// reports it.
//   <ui:Form x:Name=""SignUp"">
//       <ui:FormField x:Name=""EmailField"" Label=""Email"" Description=""We never share it."" IsRequired=""True"">
//           <ui:Input x:Name=""Email"" Placeholder=""you@example.com"" />
//       </ui:FormField>
//       <ui:Button Text=""Create account"" Clicked=""OnSubmit"" />
//   </ui:Form>
//   EmailField.Error = Email.Text.Contains('@') ? """" : ""Enter a valid email."";
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
        _required = new Microsoft.Maui.Controls.Label { Text = ""*"", FontSize = 14, FontAttributes = FontAttributes.Bold, IsVisible = false };
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
",
        [NativePlatform.Avalonia] = @"using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Form — fields stacked with space-y-6. Each FormField is a label, a control, an optional
// description and an error message; validation is yours: set a field's Error and the form
// reports it.
//   <ui:Form x:Name=""SignUp"">
//       <ui:FormField x:Name=""EmailField"" Label=""Email"" Description=""We never share it."" IsRequired=""True"">
//           <ui:Input x:Name=""Email"" Placeholder=""you@example.com"" />
//       </ui:FormField>
//       <ui:Button Text=""Create account"" Clicked=""OnSubmit"" />
//   </ui:Form>
//   EmailField.Error = Email.Text.Contains('@') ? """" : ""Enter a valid email."";
//   if (SignUp.IsValid) ...
public class Form : Border
{
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(Spacing), 24);

    private readonly StackPanel _stack = new() { Spacing = 24 };

    static Form()
    {
        SpacingProperty.Changed.AddClassHandler<Form>((f, e) => f._stack.Spacing = (double)e.NewValue!);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    // FormFields and anything else the form holds (buttons, separators).
    [Content]
    public Controls Fields => _stack.Children;

    // True when no field in the form has an Error.
    public bool IsValid => FormFields().All(f => !f.HasError);

    // The first field with an Error, top to bottom.
    public FormField? FirstInvalid => FormFields().FirstOrDefault(f => f.HasError);

    public Form()
    {
        Child = _stack;
    }

    // Every FormField in the form, however deeply it is nested.
    public IEnumerable<FormField> FormFields() => _stack.GetLogicalDescendants().OfType<FormField>();

    public void ClearErrors()
    {
        foreach (var field in FormFields()) field.Error = null;
    }
}

// One field of a Form — space-y-2: a text-sm font-medium label, the control, a muted
// description and, when Error is set, the error in text-destructive (the label turns red too).
public class FormField : Border
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<FormField, string?>(nameof(Label));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<FormField, string?>(nameof(Description));

    // The validation message; empty when the field is valid.
    public static readonly StyledProperty<string?> ErrorProperty =
        AvaloniaProperty.Register<FormField, string?>(nameof(Error), defaultBindingMode: BindingMode.TwoWay);

    // Adds a red asterisk after the label.
    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<FormField, bool>(nameof(IsRequired));

    public static readonly StyledProperty<Control?> ControlProperty =
        AvaloniaProperty.Register<FormField, Control?>(nameof(Control));

    private readonly TextBlock _label;
    private readonly TextBlock _required;
    private readonly StackPanel _labelRow;
    private readonly Border _control = new();
    private readonly TextBlock _description;
    private readonly TextBlock _error;

    static FormField()
    {
        LabelProperty.Changed.AddClassHandler<FormField>((f, _) => f.Update());
        DescriptionProperty.Changed.AddClassHandler<FormField>((f, _) => f.Update());
        ErrorProperty.Changed.AddClassHandler<FormField>((f, _) => f.Update());
        IsRequiredProperty.Changed.AddClassHandler<FormField>((f, _) => f.Update());
        ControlProperty.Changed.AddClassHandler<FormField>((f, e) => f._control.Child = e.NewValue as Control);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string? Error
    {
        get => GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    [Content]
    public Control? Control
    {
        get => GetValue(ControlProperty);
        set => SetValue(ControlProperty, value);
    }

    public bool HasError => !string.IsNullOrEmpty(Error);

    public FormField()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium };
        _required = new TextBlock { Text = ""*"", FontSize = 14, FontWeight = FontWeight.Medium, IsVisible = false };
        _required.Token(TextBlock.ForegroundProperty, ShellToken.Destructive);
        _labelRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Children = { _label, _required } };
        _description = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap, IsVisible = false };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _error = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, TextWrapping = TextWrapping.Wrap, IsVisible = false };
        _error.Token(TextBlock.ForegroundProperty, ShellToken.Destructive);
        // Screen readers announce the message when it appears.
        AutomationProperties.SetLiveSetting(_error, AutomationLiveSetting.Polite);

        Child = new StackPanel { Spacing = 8, Children = { _labelRow, _control, _description, _error } };
        Update();
    }

    private void Update()
    {
        _label.Text = Label ?? string.Empty;
        _label.Token(TextBlock.ForegroundProperty, HasError ? ShellToken.Destructive : ShellToken.Foreground);
        _required.IsVisible = IsRequired;
        _labelRow.IsVisible = !string.IsNullOrEmpty(Label);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _error.Text = Error ?? string.Empty;
        _error.IsVisible = HasError;
    }
}
"
    };
}
