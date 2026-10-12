using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

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
        _required = new TextBlock { Text = "*", FontSize = 14, FontWeight = FontWeight.Medium, IsVisible = false };
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
