namespace Photino.Blazor.Docking.LayoutScheme;

internal abstract class DockPanelContainerScheme : DockPanelBaseScheme
{
    internal virtual bool OverrideHeaderView => false;

    public virtual void ReplaceChildPanel(DockPanelBaseScheme oldPanel, DockPanelBaseScheme newPanel)
    {
        newPanel.StoreComponentsStateInside();
    }
    public virtual void DetachChildPanel(DockPanelScheme detachingPanel, out DockPanelBaseScheme lastPanel)
    {
        lastPanel = null;
    }
}
