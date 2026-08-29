namespace MAUI.Demo.Components.UI;

public partial class Breadcrumb : HorizontalStackLayout
{
    public Breadcrumb()
    {
        Spacing = 0;
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        Dispatcher.Dispatch(RefreshItemSeparators);
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        Dispatcher.Dispatch(RefreshItemSeparators);
    }

    private void RefreshItemSeparators()
    {
        var items = Children.OfType<BreadcrumbItem>().ToList();
        for (int i = 0; i < items.Count; i++)
            items[i].SetSeparatorVisible(i < items.Count - 1);
    }
}
