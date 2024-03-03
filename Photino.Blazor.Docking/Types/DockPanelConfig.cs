namespace Photino.Blazor.Docking;

/// <summary>
/// Dock panel configuration.
/// </summary>
public struct DockPanelConfig
{
    /// <summary>
    /// Non-repeating type of blazor component to display inside dock panel.
    /// </summary>
    public Type ComponentType { get; init; }

    /// <summary>
    /// Unique panel identificator.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Title displayed in panel header.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Is panel can be detached from its location or not.
    /// </summary>
    public bool CanBeDetached { get; init; }

    /// <summary>
    /// Disabled compass zones for this panel.
    /// </summary>
    public DockZone DisabledZones { get; init; }

    /// <param name="componentType">Non-repeating type of blazor component to display inside dock panel</param>
    /// <param name="id">Unique panel identificator</param>
    /// <param name="title">Localized title displayed in panel header</param>
    /// <param name="canBeDetached">Is panel can be detached from its location or not</param>
    /// <param name="disabledZones">Disabled compass zones for this panel</param>
    public DockPanelConfig(Type componentType, string id, string title,  bool canBeDetached = true, DockZone disabledZones = default)
    {
        ComponentType = componentType;
        Id = id;
        Title = title;
        CanBeDetached = canBeDetached;
        DisabledZones = disabledZones;
    }
}
