using System.Drawing;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class HostWindowInfo
{
    public Point Location { get; set; }
    public Size Size { get; set; }
    public bool Maximized { get; set; }
}
