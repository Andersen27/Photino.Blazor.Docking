using System.Drawing;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockPanelTabsScheme : DockPanelContainerScheme
{
    internal override bool OverrideHeaderView => VisiblePanels.Count() > 1;

    private List<DockPanelScheme> _panels = [];
    public List<DockPanelScheme> Panels
    {
        get => _panels;
        set
        {
            _panels = value;
            NotifyPropertyChanged(nameof(Panels));
        }
    }

    public IEnumerable<DockPanelScheme> VisiblePanels => Panels.Where(p => !p.IsHidden);

    private int _currentTab = 0;
    public int CurrentTabIndex
    {
        get => _currentTab;
        set
        {
            if (_currentTab != value)
            {
                _currentTab = value;
                NotifyPropertyChanged(nameof(CurrentTabIndex));
                NotifyPropertyChanged(nameof(CurrentTab));
            }
        }
    }

    [JsonIgnore]
    public DockPanelScheme CurrentTab
    {
        get => VisiblePanels.Any() ? Panels[CurrentTabIndex] : null;
        set => CurrentTabIndex = Math.Max(0, Panels.IndexOf(value));
    }

    public override void ReplaceChildPanel(DockPanelBaseScheme oldPanel, DockPanelBaseScheme newPanel)
    {
        if (oldPanel is not DockPanelScheme oldTab || newPanel is not DockPanelScheme newTab)
            throw new NotSupportedException();

        base.ReplaceChildPanel(oldPanel, newPanel);
        var oldTabIndex = Panels.IndexOf(oldTab);
        if (oldTabIndex != -1)
        {
            Panels[oldTabIndex] = newTab;
            NotifyPropertyChanged(nameof(Panels));
        }
    }

    public override void DetachChildPanel(DockPanelScheme detachingPanel, out DockPanelBaseScheme lastPanel)
    {
        #if DEBUG
        if (Panels.Count < 2) throw new Exception();
        #endif

        var panelRemoved = false;
        DockPanelScheme prevPanel = null, nextPanel = null;
        foreach (var panel in VisiblePanels.ToArray())
        {
            if (panelRemoved)
            {
                if (prevPanel != null || nextPanel != null)
                    panel.StoreComponentState();
                nextPanel ??= panel;
            }
            else
            {
                if (panel == detachingPanel)
                    panelRemoved = Panels.Remove(panel);
                else
                    prevPanel = panel;
            }
        }
        CurrentTab = nextPanel ?? prevPanel;
        lastPanel = Panels.Count > 1 ? null : Panels.First();

        NotifyPropertyChanged(nameof(Panels));

        if (!VisiblePanels.Any())
            ParentContainer.NotifyPropertyChanged(nameof(ComputedIsHidden));
    }

    public override IEnumerable<DockPanelScheme> GetAllDockPanelsInside()
    {
        return Panels;
    }

    public override DockPanelScheme FindDockPanel(string id)
    {
        return Panels.FirstOrDefault(p => p.Id == id);
    }

    [JsonIgnore]
    public override Size ComputedMinSize => CurrentTab?.ComputedMinSize ?? Size.Empty;
}
