namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Content))]
public partial class CollapsibleContent : ContentView
{
    private Collapsible? _parent;

    public CollapsibleContent()
    {
        IsVisible = false;
        Opacity = 0;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Detach from any previous parent to avoid duplicate handlers if the view is reparented.
        if (_parent != null)
            _parent.OpenChanged -= OnParentOpenChanged;

        _parent = this.FindParentOfType<Collapsible>();
        if (_parent != null)
        {
            _parent.OpenChanged += OnParentOpenChanged;
            ApplyState(_parent.Open, animate: false);
        }
    }

    private void OnParentOpenChanged(object? sender, bool open) => _ = AnimateAsync(open);

    private async Task AnimateAsync(bool open)
    {
        if (open)
        {
            IsVisible = true;
            await this.FadeTo(1, 150, Easing.CubicOut);
        }
        else
        {
            await this.FadeTo(0, 150, Easing.CubicIn);
            IsVisible = false;
        }
    }

    private void ApplyState(bool open, bool animate)
    {
        if (animate) { _ = AnimateAsync(open); return; }
        IsVisible = open;
        Opacity = open ? 1 : 0;
    }
}
