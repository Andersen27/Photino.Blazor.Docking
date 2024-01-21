using System.Drawing;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockPanelFloatScheme : DockPanelHostScheme
{
    private Point _windowLocation;
    public Point WindowLocation
    {
        get => _windowLocation;
        set
        {
            _windowLocation = value;
            NotifyPropertyChanged(nameof(WindowLocation));
        }
    }

    public Size WindowSize { get; set; }
    public bool WindowMaximized { get; set; }
    public Size WindowMinSize => HostedPanel.ComputedMinSize;

    [JsonIgnore]
    public bool MarkedToDestroy { get; set; }

    [JsonIgnore]
    public bool MarkedAsAttaching { get; private set; }

    public void Destroy(bool attaching = false)
    {
        MarkedToDestroy = true;
        MarkedAsAttaching = attaching;
        NotifyPropertyChanged(nameof(MarkedToDestroy));
    }
}
