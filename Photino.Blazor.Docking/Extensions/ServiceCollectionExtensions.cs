using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor.CustomWindow.Extensions;
using Photino.Blazor.CustomWindow.Services;
using Photino.Blazor.Docking.Services;
using System.Drawing;

namespace Photino.Blazor.Docking.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure and add <see cref="DockingService"/> as singleton to <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="servicesInitializer">
    /// Initialization function for services, called for <see cref="PhotinoBlazorAppBuilder"/> instances of floating panels.
    /// Function should add to the collection all services required by Blazor components inside floating panels, excluding <see cref="DockingService"/>.
    /// </param>
    /// <param name="panelsConfig">
    /// Dock panels configuration set.
    /// For each panel specify at least a non-repeating Blazor component type, a unique identificator and a localized title.
    /// </param>
    /// <param name="floatPanelWrapperComponent">
    /// The root wrapper component for each float panel.
    /// The layout of child panels will be rendered as a nested <see cref="RenderFragment"/> of the specified component.
    /// Must be subclass of <see cref="ComponentBase"/> and have ChildContent parameter.
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
                                                             Type floatPanelWrapperComponent = null,
                                                             string multiplePanelsTitle = "",
                                                             bool restoreHostWindowOnOpen = true,
                                                             Size? panelsMinSize = null,
                                                             Size? defaultFloatPanelSize = null)
    {
        services.AddCustomWindow();
        
        services.AddSingleton(sp =>
        {
            var screensAgentService = sp.GetRequiredService<ScreensAgentService>();
            return new DockingService(
                screensAgentService,
                servicesInitializer,
                panelsConfig,
                floatPanelWrapperComponent,
                multiplePanelsTitle,
                restoreHostWindowOnOpen,
                panelsMinSize,
                defaultFloatPanelSize
            );
        });
        return services;
    }
}