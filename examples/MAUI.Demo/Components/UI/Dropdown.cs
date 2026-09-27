namespace MAUI.Demo.Components.UI;

// Dropdown menu. The content floats below the trigger over whatever follows.
//   <ui:Dropdown>
//       <ui:DropdownTrigger><ui:Button Text="Options" Variant="Outline" /></ui:DropdownTrigger>
//       <ui:DropdownContent>
//           <ui:DropdownItem Text="Profile" Icon="User" Clicked="OnProfile" />
//       </ui:DropdownContent>
//   </ui:Dropdown>
public partial class Dropdown : ShellAnchorLayout, IShellPopup
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Dropdown), false,
            propertyChanged: (b, o, n) => ((Dropdown)b).OnOpenChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    private Action? _restoreZ;

    public Dropdown()
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
        var content = Children.OfType<DropdownContent>().FirstOrDefault();
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
