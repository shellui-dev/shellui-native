namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Content))]
public partial class AccordionContent : ContentView
{
    private AccordionItem? _item;

    public AccordionContent()
    {
        IsVisible = false;
        Opacity = 0;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_item != null)
            _item.OpenChanged -= OnItemOpenChanged;

        _item = this.FindParentOfType<AccordionItem>();
        if (_item != null)
        {
            _item.OpenChanged += OnItemOpenChanged;
            IsVisible = _item.IsOpen;
            Opacity = _item.IsOpen ? 1 : 0;
        }
    }

    private void OnItemOpenChanged(object? sender, bool open) => _ = AnimateAsync(open);

    private async Task AnimateAsync(bool open)
    {
        if (open)
        {
            IsVisible = true;
            await this.FadeToAsync(1, 150, Easing.CubicOut);
        }
        else
        {
            await this.FadeToAsync(0, 150, Easing.CubicIn);
            IsVisible = false;
        }
    }
}
