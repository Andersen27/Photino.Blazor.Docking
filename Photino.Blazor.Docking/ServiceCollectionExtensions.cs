using Photino.Blazor.Docking;
using System;
using System.Drawing;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure and add <see cref="DockingService"/> as singleton to <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="servicesInitializer">
    /// Initialization function for services, called for <see cref="Photino.Blazor.PhotinoBlazorAppBuilder"/> instances of floating panels.
    /// Function should add to the collection all services required by Blazor components inside floating panels, excluding <see cref="DockingService"/>.
    /// </param>
    /// <param name="panelsConfig">
    /// Dock panels configuration set.
    /// For each panel specify at least a non-repeating Blazor component type, a unique identificator and a localized title.
    /// </param>
    /// <param name="multiplePanelsTitle">
    /// Title displayed in the header of floating panel with more than one visible dock panels inside.
    /// </param>
    /// <param name="restoreHostWindowOnOpen">
    /// Indicates whether to restore host window location from the docking layout on the first opening.
    /// </param>
    /// <param name="panelsMinSize">
    /// Dock panels minimum size in screen pixels. For <c>null</c> is <see cref="Size"/>(100, 100).
    /// </param>
    /// <param name="defaultFloatPanelSize">
    /// Default floating window size in screen pixels. For <c>null</c> is <see cref="Size"/>(400, 600).
    /// </param>
    public static IServiceCollection AddPhotinoBlazorDocking(this IServiceCollection services,
                                                             Action<IServiceCollection> servicesInitializer,
                                                             DockPanelConfig[] panelsConfig,
                                                             string multiplePanelsTitle = "",
                                                             bool restoreHostWindowOnOpen = true,
                                                             Size? panelsMinSize = null,
                                                             Size? defaultFloatPanelSize = null)
    {
        var dockingService = new DockingService (
            servicesInitializer,
            panelsConfig,
            multiplePanelsTitle,
            restoreHostWindowOnOpen,
            panelsMinSize,
            defaultFloatPanelSize
        );
        services.AddSingleton(dockingService);
        return services;
    }
}