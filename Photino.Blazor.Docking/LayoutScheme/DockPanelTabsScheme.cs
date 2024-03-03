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
        get => VisiblePanels.Count() > 0 ? Panels[CurrentTabIndex] : null;
        set => CurrentTabIndex = Panels.IndexOf(value);
    }

    public override void ReplaceChildPanel(DockPanelBaseScheme oldPanel, DockPanelBaseScheme newPanel)
    {
        if (oldPanel is not DockPanelScheme oldTab || newPanel is not DockPanelScheme newTab)
            throw new NotSupportedException();

        var oldTabIndex = Panels.IndexOf(oldTab);
        if (oldTabIndex != -1)
        {
            Panels[oldTabIndex] = newTab;
            NotifyPropertyChanged(nameof(Panels));
        }
    }

    public override void DetachChildPanel(DockPanelScheme detachingPanel, out DockPanelBaseScheme lastPanel)
    {
        lastPanel = null;
        Panels.Remove(detachingPanel);
        if (CurrentTabIndex > Panels.Count - 1)
            CurrentTabIndex -= 1;

        if (Panels.Count == 1)
            lastPanel = Panels.First();

        NotifyPropertyChanged(nameof(Panels));
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
