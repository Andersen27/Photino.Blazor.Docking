namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockingLayout
{
    private HostWindowInfo _hostWindow;
    public HostWindowInfo HostWindow
    {
        get => _hostWindow;
        set => _hostWindow ??= value;
    }

    public DockPanelHostScheme HostPanel { get; set; } = null;

    public List<DockPanelFloatScheme> FloatPanels { get; set; } = [];

    public DockPanelScheme FindDockPanel(string id, out DockPanelHostScheme hostPanel)
    {
        if (HostPanel?.FindDockPanel(id) is DockPanelScheme p)
        {
            hostPanel = HostPanel;
            return p;
        }

        foreach (var floatPanel in FloatPanels)
        {
            if (floatPanel.HostedPanel.FindDockPanel(id) is DockPanelScheme fp)
            {
                hostPanel = floatPanel;
                return fp;
            }
        }

        hostPanel = null;
        return null;
    }
}
