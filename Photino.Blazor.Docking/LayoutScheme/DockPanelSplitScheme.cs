using System.Drawing;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockPanelSplitScheme : DockPanelContainerScheme
{
    internal override bool OverrideHeaderView => ((FirstPanel?.ComputedIsHidden ?? true) || (SecondPanel?.ComputedIsHidden ?? true))
                                                 && ParentContainer.OverrideHeaderView;

    public DockPanelBaseScheme FirstPanel { get; set; }
    public DockPanelBaseScheme SecondPanel { get; set; }
    public SplitOrientation Orientation { get; set; } = SplitOrientation.Horizontal;
    public double Percent { get; set; } = 0.5;

    public override void ReplaceChildPanel(DockPanelBaseScheme oldPanel, DockPanelBaseScheme newPanel)
    {
        base.ReplaceChildPanel(oldPanel, newPanel);
        if (FirstPanel == oldPanel)
        {
            FirstPanel = newPanel;
            newPanel.ParentContainer = this;
            NotifyPropertyChanged(nameof(FirstPanel));
        }
        else if (SecondPanel == oldPanel)
        {
            SecondPanel = newPanel;
            newPanel.ParentContainer = this;
            NotifyPropertyChanged(nameof(SecondPanel));
        }
    }

    public override void DetachChildPanel(DockPanelScheme detachingPanel, out DockPanelBaseScheme lastPanel)
    {
        if (FirstPanel == detachingPanel)
        {
            FirstPanel = null;
            lastPanel = SecondPanel;
            NotifyPropertyChanged(nameof(FirstPanel));
        }
        else
        {
            SecondPanel = null;
            lastPanel = FirstPanel;
            NotifyPropertyChanged(nameof(SecondPanel));
        }
    }

    public override IEnumerable<DockPanelScheme> GetAllDockPanelsInside()
    {
        foreach(var child in FirstPanel.GetAllDockPanelsInside())
            yield return child;

        foreach (var child in SecondPanel.GetAllDockPanelsInside())
            yield return child;

        yield break;
    }

    public override DockPanelScheme FindDockPanel(string id)
    {
        return FirstPanel.FindDockPanel(id) ?? SecondPanel.FindDockPanel(id);
    }

    [JsonIgnore]
    public override Size ComputedMinSize
    {
        get
        {
            var fpMinSize = FirstPanel?.ComputedMinSize ?? Size.Empty;
            var spMinSize = SecondPanel?.ComputedMinSize ?? Size.Empty;

            return Orientation == SplitOrientation.Horizontal ?
                new Size(fpMinSize.Width + spMinSize.Width, Math.Max(fpMinSize.Height, spMinSize.Height)) :
                new Size(Math.Max(fpMinSize.Width, spMinSize.Width), fpMinSize.Height + spMinSize.Height);
        }
    }
}
