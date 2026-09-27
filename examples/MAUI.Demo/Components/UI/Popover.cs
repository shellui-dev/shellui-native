namespace MAUI.Demo.Components.UI;

// Popover — a floating panel below the trigger.
//   <ui:Popover>
//       <ui:PopoverTrigger><ui:Button Text="Info" Variant="Outline" /></ui:PopoverTrigger>
//       <ui:PopoverContent>...</ui:PopoverContent>
//   </ui:Popover>
public partial class Popover : ShellAnchorLayout, IShellPopup
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Popover), false,
            propertyChanged: (b, o, n) => ((Popover)b).OnOpenChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    private Action? _restoreZ;

    public Popover()
    {
        HorizontalOptions = LayoutOptions.Start;
    }

    public void SetOpen(bool value) => IsOpen = value;
    public void Toggle() => IsOpen = !IsOpen;
    public void Close() => IsOpen = false;

    // Kept for compatibility with earlier versions.
    public void ToggleAsync() => Toggle();
    public void CloseAsync() => Close();

    private async void OnOpenChanged()
    {
        IsOpenChanged?.Invoke(this, IsOpen);
        var content = Children.OfType<PopoverContent>().FirstOrDefault();
        if (IsOpen)
        {
            _restoreZ?.Invoke();
            _restoreZ = ShellPopups.RaiseAboveSiblings(this);
            ShellPopups.Opened(this);
            if (content != null) await ShellPopups.AnimateAsync(content, true);
        }
        else
        {
            ShellPopups.Closed(this);
            if (content != null) await ShellPopups.AnimateAsync(content, false);
            if (!IsOpen) { _restoreZ?.Invoke(); _restoreZ = null; }
        }
    }
}
