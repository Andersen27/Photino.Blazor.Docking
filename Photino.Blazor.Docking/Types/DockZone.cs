namespace Photino.Blazor.Docking;

/// <summary>
/// Compass zones for panel attaching.
/// <br><see cref="Left"/> and <see cref="Right"/> for horizontal split,</br>
/// <br><see cref="Top"/> and <see cref="Bottom"/> for vertical split,</br>
/// <br><see cref="Center"/> for group into a tabbed panel.</br>
/// <br><b>Specials:</b> <see cref="Outside"/> for floating panel, <see cref="All"/> includes all zones.</br>
/// </summary>
[Flags]
public enum DockZone
{
    Outside = 0,
    Center = 1,
    Left = 2,
    Top = 4,
    Right = 8,
    Bottom = 16,
    All = 31,
}
