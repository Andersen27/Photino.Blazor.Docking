using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

[JsonDerivedType(typeof(DockPanelSplitScheme), "DockPanelSplit")]
[JsonDerivedType(typeof(DockPanelTabsScheme), "DockPanelTabs")]
[JsonDerivedType(typeof(DockPanelScheme), "DockPanel")]
internal abstract class DockPanelBaseScheme : INotifyPropertyChanged
{
    [JsonIgnore]
    public DockPanelContainerScheme ParentContainer { get; set; } = null;

    [JsonIgnore]
    public bool ComputedIsHidden => GetAllDockPanelsInside().All(p => p.IsHidden);

    [JsonIgnore]
    public virtual Size ComputedMinSize => Size.Empty;


    public virtual DockPanelScheme FindDockPanel(string id) => null;
    public virtual IEnumerable<DockPanelScheme> GetAllDockPanelsInside()
    {
        yield break;
    }
    public void StoreComponentsStateInside()
    {
        foreach (var dockPanel in GetAllDockPanelsInside().Where(d => !d.IsHidden))
            dockPanel.StoreComponentState();
    }

    protected IEnumerable<DockPanelContainerScheme> GetParentsChain()
    {
        if (ParentContainer is null)
        {
            yield break;
        }
        else
        {
            yield return ParentContainer;
            foreach (var parent in ParentContainer.GetParentsChain())
                yield return parent;
        }
    }

    private DockPanelContainerScheme GetTopParent(DockPanelContainerScheme lastParent) => ParentContainer?.GetTopParent(ParentContainer) ?? lastParent;
    public DockPanelHostScheme GetTopParent() => GetTopParent(ParentContainer) as DockPanelHostScheme;

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    internal void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
