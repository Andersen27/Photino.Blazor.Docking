using Photino.Blazor.Docking.LayoutScheme;

namespace Photino.Blazor.Docking;

internal sealed class DockAttachInfo
{
    public DockPanelScheme Panel { get; set; }
    public DockZone Zone { get; set; }
}
