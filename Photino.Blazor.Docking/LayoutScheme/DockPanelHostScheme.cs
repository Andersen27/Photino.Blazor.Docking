namespace Photino.Blazor.Docking.LayoutScheme;

internal class DockPanelHostScheme : DockPanelContainerScheme
{
    internal override bool OverrideHeaderView => true;

    public DockPanelBaseScheme HostedPanel { get; set; }

    public override void ReplaceChildPanel(DockPanelBaseScheme oldPanel, DockPanelBaseScheme newPanel)
    {
        if (HostedPanel == oldPanel)
        {
            HostedPanel = newPanel;
            newPanel.ParentContainer = this;
            NotifyPropertyChanged(nameof(HostedPanel));
        }
    }
    public override DockPanelScheme FindDockPanel(string id)
    {
        return HostedPanel.FindDockPanel(id);
    }

    public override IEnumerable<DockPanelScheme> GetAllDockPanelsInside()
    {
        return HostedPanel.GetAllDockPanelsInside();
    }
}
