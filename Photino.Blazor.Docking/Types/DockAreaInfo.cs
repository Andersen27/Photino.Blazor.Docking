using Photino.Blazor.Docking.LayoutScheme;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Photino.Blazor.Docking;

internal struct DockAreaInfo
{
    private const int CompassSize = 112;
    private const int CompassCenterZoneSize = 38;

    private DockZone _disabledZones;
    private Dictionary<DockZone, Rectangle> _compassZonesBounds = [];

    public int OrderIndex { get; set; }
    public DockPanelScheme Panel { get; set; }
    public Rectangle Area { get; set; }
    public double AreaScaleFactor { get; set; }
    public readonly ReadOnlyDictionary<DockZone, Rectangle> ZonesBounds => _compassZonesBounds.AsReadOnly();

    public DockAreaInfo(int orderIndex, DockPanelScheme panel, Rectangle area, double areaScaleFactor, DockZone disabledZones)
    {
        OrderIndex = orderIndex;
        Panel = panel;
        Area = area;
        AreaScaleFactor = areaScaleFactor;
        _disabledZones = disabledZones;
        CalcCompassZonesBounds();
    }

    private void CalcCompassZonesBounds()
    {
        var actualCompassSize = (int)(CompassSize * AreaScaleFactor);
        var actualCompassCenterZoneSize = (int)(CompassCenterZoneSize * AreaScaleFactor);
        var compassLocation = new Point((Area.Width - actualCompassSize) / 2, (Area.Height - actualCompassSize) / 2);
        compassLocation.Offset(Area.Location);
        var centerOffset = (actualCompassSize - actualCompassCenterZoneSize) / 2;

        if (!Panel.IsDetachedGhost)
        {
            if ((_disabledZones & DockZone.Left) != DockZone.Left)
                _compassZonesBounds[DockZone.Left] = new Rectangle(compassLocation.X, compassLocation.Y + centerOffset,
                                                                   centerOffset, actualCompassCenterZoneSize);

            if ((_disabledZones & DockZone.Right) != DockZone.Right)
                _compassZonesBounds[DockZone.Right] = new Rectangle(compassLocation.X + actualCompassSize - centerOffset, compassLocation.Y + centerOffset,
                                                                    centerOffset, actualCompassCenterZoneSize);

            if ((_disabledZones & DockZone.Top) != DockZone.Top)
                _compassZonesBounds[DockZone.Top] = new Rectangle(compassLocation.X + centerOffset, compassLocation.Y,
                                                                  actualCompassCenterZoneSize, centerOffset);

            if ((_disabledZones & DockZone.Bottom) != DockZone.Bottom)
                _compassZonesBounds[DockZone.Bottom] = new Rectangle(compassLocation.X + centerOffset, compassLocation.Y + actualCompassSize - centerOffset,
                                                                     actualCompassCenterZoneSize, centerOffset);
        }
        
        if ((_disabledZones & DockZone.Center) != DockZone.Center)
            _compassZonesBounds[DockZone.Center] = new Rectangle(compassLocation.X + centerOffset, compassLocation.Y + centerOffset,
                                                                 actualCompassCenterZoneSize, actualCompassCenterZoneSize);
    }
}
