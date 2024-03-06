using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor.CustomWindow.Services;
using Photino.Blazor.Docking.Components.Internal;
using Photino.Blazor.Docking.LayoutScheme;
using System.Drawing;
using System.Text.Json;

namespace Photino.Blazor.Docking.Services;

/// <summary>
/// Singleton service for multi-window docking functionality.
/// Implements panel arrangement with a horizontal/vertical splitter,
/// grouping into a tabbed panel and detaching panel into a separate floating window.
/// Supports hiding/showing panels by id and saving/loading docking layout in JSON format.
/// </summary>
public sealed class DockingService
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
    };

    private ScreensAgentService _screensAgentService;
    private Action<IServiceCollection> _servicesInitializer;
    private DockPanelConfig[] _panelsConfig = null;
    private Size _defaultFloatPanelSize = new Size(400, 600);
    private Point _defaultFloatPanelLocationOffset = new Point(50, 50);
    private DockingLayout _dockingLayout = new();
    private List<DockAreaInfo> _orderedDockPanelsAreaInfo = [];
    private List<DockPanelHostScheme> _hostPanelsByVisibleOrder = [];
    private Dictionary<DockPanelFloatScheme, PhotinoBlazorApp> _floatPanelApps = [];

    internal Type FloatPanelWrapperComponent { get; private set; } = null;
    internal string MultiplePanelsTitle { get; private init; } = string.Empty;
    internal bool RestoreHostWindow { get; private init; } = true;
    internal DockAttachInfo DockToAttach { get; private set; } = new();
    internal DockPanelFloatScheme MovingFloatPanel { get; private set; }
    internal DockZone GlobalDisabledDockZones { get; private set; }
    internal Queue<DockPanelFloatScheme> FloatPanelsCreateQueue { get; private set; } = new();
    internal DockPanelHostScheme HostPanel
    {
        get => _dockingLayout.HostPanel;
        set => _dockingLayout.HostPanel = value;
    }
    internal HostWindowInfo HostWindow
    {
        get => _dockingLayout.HostWindow;
        set => _dockingLayout.HostWindow = value;
    }
    internal List<DockPanelFloatScheme> FloatPanels
    {
        get => _dockingLayout.FloatPanels;
        set => _dockingLayout.FloatPanels = value;
    }

    internal event Action DockPanelMoveBegin;
    internal event Action DockPanelToAttachChanged;
    internal event Action DockZoneToAttachChanged;

    public event Action LayoutLoaded;
    public event Action<string> DockPanelClosed;

    internal DockingService(ScreensAgentService screensAgentService,
                            Action<IServiceCollection> servicesInitializer,
                            DockPanelConfig[] panelsConfig,
                            Type floatPanelWrapperComponent = null,
                            string multiplePanelsTitle = "",
                            bool restoreHostWindowOnOpen = true,
                            Size? panelsMinSize = null,
                            Size? defaultFloatPanelSize = null)
    {
        foreach (var panelConfig in panelsConfig)
            if (panelsConfig.Count(p => p.Id == panelConfig.Id || p.ComponentType == panelConfig.ComponentType) > 1)
                throw new Exception("Invalid docking service configuration: " +
                    "there are duplicates of identificators or panel types in the dock panels configuration set.");

        if (!floatPanelWrapperComponent?.IsSubclassOf(typeof(ComponentBase)) ?? false)
            throw new Exception("Invalid docking service configuration: " +
                "floatPanelWrapperComponent type must be subclass of ComponentBase.");

        _screensAgentService = screensAgentService;
        _servicesInitializer = servicesInitializer;
        _panelsConfig = panelsConfig;
        FloatPanelWrapperComponent = floatPanelWrapperComponent;
        MultiplePanelsTitle = multiplePanelsTitle;
        RestoreHostWindow = restoreHostWindowOnOpen;

        if (panelsMinSize.HasValue)
            DockPanelScheme.MinSize = panelsMinSize.Value;
        if (defaultFloatPanelSize.HasValue)
            _defaultFloatPanelSize = defaultFloatPanelSize.Value;
    }

    private void CreateFloatPanel(DockPanelFloatScheme floatPanel)
    {
        FloatPanelsCreateQueue.Enqueue(floatPanel);

        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault();
        appBuilder.Services.AddSingleton(this);
        appBuilder.Services.AddSingleton(_screensAgentService);
        _servicesInitializer(appBuilder.Services);
        appBuilder.RootComponents.Add<DockPanelFloat>("app");
        var app = appBuilder.Build();

        app.MainWindow.Chromeless = true;
        app.MainWindow.WindowCreated += (_, _) => app.MainWindow.Size = Size.Empty;

        // hack to avoid application crashes on docking panel detaching
        // reference to PhotinoBlazorApp instance must be stored
        _floatPanelApps[floatPanel] = app;

        app.Run();
    }

    internal void ReleaseGhostPanel(DockPanelScheme panel)
    {
        var parent = panel.ParentContainer;
        parent.DetachChildPanel(panel, out var lastPanel);
        if (lastPanel != null)
        {
            parent.ParentContainer.ReplaceChildPanel(parent, lastPanel);
        }
    }

    internal DockPanelFloatScheme DetachPanel(DockPanelScheme panel, Point mousePos, Point headerOffset)
    {
        var hostedPanel = (DockPanelScheme)panel.Clone();
        var floatPanel = new DockPanelFloatScheme()
        {
            HostedPanel = hostedPanel,
            WindowLocation = new Point(mousePos.X - headerOffset.X, mousePos.Y - headerOffset.Y),
        };
        hostedPanel.ParentContainer = floatPanel;
        panel.IsDetachedGhost = true;

        FloatPanels.Add(floatPanel);
        CreateFloatPanel(floatPanel);
        return floatPanel;
    }

    internal void AttachPanel(DockPanelBaseScheme attachingPanel, DockPanelScheme targetPanel, DockZone attachingZone)
    {
        DockPanelBaseScheme actualTargetPanel = targetPanel.ParentContainer is DockPanelTabsScheme tabsPanelParent ?
                tabsPanelParent : targetPanel;

        if (targetPanel.IsDetachedGhost)
        {
            targetPanel.IsDetachedGhost = false;
        }
        else if(attachingZone == DockZone.Center)
        {
            var attachingDockPanel = (DockPanelScheme)attachingPanel;
            if (targetPanel.ParentContainer is DockPanelTabsScheme tabsPanel)
            {
                attachingDockPanel.StoreComponentState();
                tabsPanel.Panels.Add(attachingDockPanel);
                tabsPanel.CurrentTabIndex = tabsPanel.Panels.Count - 1;
            }
            else
            {
                var targetPanelParent = targetPanel.ParentContainer;
                var newTabsPanel = new DockPanelTabsScheme()
                {
                    Panels = [targetPanel, attachingDockPanel],
                    CurrentTabIndex = 1,
                };
                targetPanel.ParentContainer = newTabsPanel;
                attachingDockPanel.ParentContainer = newTabsPanel;

                targetPanelParent.ReplaceChildPanel(targetPanel, newTabsPanel);
            }
        }
        else
        {
            var targetPanelParent = actualTargetPanel.ParentContainer;
            var newSplitPanel = new DockPanelSplitScheme() {
                FirstPanel = attachingZone is DockZone.Left or DockZone.Top ? attachingPanel : actualTargetPanel,
                SecondPanel = attachingZone is DockZone.Right or DockZone.Bottom ? attachingPanel : actualTargetPanel,
                Orientation = attachingZone is DockZone.Left or DockZone.Right ? SplitOrientation.Horizontal : SplitOrientation.Vertical,
            };
            actualTargetPanel.ParentContainer = newSplitPanel;
            attachingPanel.ParentContainer = newSplitPanel;

            targetPanelParent.ReplaceChildPanel(actualTargetPanel, newSplitPanel);
        }
    }

    internal void FloatPanelMoveBegin(DockPanelFloatScheme floatPanel)
    {
        MovingFloatPanel = floatPanel;
        GlobalDisabledDockZones = floatPanel.HostedPanel is DockPanelContainerScheme ? DockZone.Center : 0;
        DockPanelMoveBegin?.Invoke();
    }

    internal void FloatPanelMoveEnd()
    {
        if (DockToAttach.Zone != DockZone.Outside)
        {
            AttachPanel(MovingFloatPanel.HostedPanel, DockToAttach.Panel, DockToAttach.Zone);
            MovingFloatPanel.Destroy(true);
        }

        DockToAttach.Panel = null;
        DockToAttach.Zone = DockZone.Outside;
        MovingFloatPanel = null;
        GlobalDisabledDockZones = 0;
        _orderedDockPanelsAreaInfo.Clear();

        DockPanelToAttachChanged?.Invoke();
    }

    internal async Task FloatPanelMovingAsync(PointerEventArgs e)
    {
        DockPanelScheme newPanel = default;
        DockZone newZone = default;

        var pointerPos = await _screensAgentService.GetOSPointerPositionAsync(e);
        foreach(var areaInfo in _orderedDockPanelsAreaInfo)
        {
            if (!areaInfo.Area.Contains(pointerPos))
                continue;

            newPanel = areaInfo.Panel;
            foreach (var (zone, bounds) in areaInfo.ZonesBounds)
            {
                if (bounds.Contains(pointerPos))
                {
                    newZone = zone;
                    break;
                }
            }
            break;
        }

        if (DockToAttach.Panel != newPanel)
        {
            DockToAttach.Panel = newPanel;
            DockPanelToAttachChanged?.Invoke();
        }
        if (DockToAttach.Zone != newZone)
        {
            DockToAttach.Zone = newZone;
            DockZoneToAttachChanged?.Invoke();
        }
    }

    internal void FloatPanelClosing(DockPanelFloatScheme floatPanel)
    {
        if (floatPanel.MarkedAsAttaching)
        {
            FloatPanels.Remove(floatPanel);
        }
        else
        {
            foreach (var child in floatPanel.GetAllDockPanelsInside())
                CloseDockPanel(child);
        }

        _floatPanelApps.Remove(floatPanel);
        _hostPanelsByVisibleOrder.Remove(floatPanel);
    }

    internal void CloseDockPanel(DockPanelScheme panel)
    {
        panel.IsHidden = true;
        DockPanelClosed?.Invoke(panel.Id);
    }

    internal void PutHostPanelOnTopOfVisibleList(DockPanelHostScheme hostPanel)
    {
        _hostPanelsByVisibleOrder.Remove(hostPanel);
        _hostPanelsByVisibleOrder.Insert(0, hostPanel);
    }

    internal void StoreDockPanelAreaInfo(DockPanelScheme dockPanel, Rectangle area, double areaScaleFactor)
    {
        if (dockPanel.IsDetachedGhost)
        {
            MovingFloatPanel.WindowSize = area.Size;
        }
        if (dockPanel.GetTopParent() != MovingFloatPanel)
        {
            var orderIndex = _hostPanelsByVisibleOrder.IndexOf(dockPanel.GetTopParent());
            var disabledZones = GetPanelConfig(dockPanel.Id).DisabledZones | GlobalDisabledDockZones;
            var dockAreaInfo = new DockAreaInfo(orderIndex, dockPanel, area, areaScaleFactor, disabledZones);

            var insertIndex = _orderedDockPanelsAreaInfo.Count > 0 && _orderedDockPanelsAreaInfo.First().OrderIndex < dockAreaInfo.OrderIndex ?
                _orderedDockPanelsAreaInfo.FindIndex(i => i.OrderIndex >= dockAreaInfo.OrderIndex) : 0;
            if (insertIndex == -1)
                insertIndex = _orderedDockPanelsAreaInfo.Count;

            _orderedDockPanelsAreaInfo.Insert(insertIndex, dockAreaInfo);
        }
    }

    internal DockPanelConfig GetPanelConfig(Type componentType) => _panelsConfig.Single(p => p.ComponentType == componentType);

    /// <summary>
    /// Gets dock panel configuration.
    /// </summary>
    /// <param name="id">Target dock panel identificator</param>
    public DockPanelConfig GetPanelConfig(string id) => _panelsConfig.Single(p => p.Id == id);

    /// <summary>
    /// Gets all exists dock panel configurations.
    /// </summary>
    public IReadOnlyCollection<DockPanelConfig> GetPanelsConfig() => _panelsConfig.AsReadOnly();

    /// <summary>
    /// Show dock panel in its last known location or in a floating window if panel is not defined in the layout.
    /// </summary>
    /// <param name="id">Target dock panel identificator</param>
    public void ShowPanel(string id)
    {
        if (_dockingLayout.FindDockPanel(id, out var hostPanel) is DockPanelScheme panel)
        {
            var floatPanel = hostPanel as DockPanelFloatScheme;
            var topPanelIsFloatAndHidden = floatPanel?.ComputedIsHidden ?? false;
            panel.IsHidden = false;

            if (topPanelIsFloatAndHidden)
            {
                floatPanel.MarkedToDestroy = false;
                CreateFloatPanel(floatPanel);
            }
        }
        else
        {
            var hostedPanel = new DockPanelScheme() { Id = id };
            var floatPanel = new DockPanelFloatScheme()
            {
                HostedPanel = hostedPanel,
                WindowLocation = new Point(HostWindow.Location.X + _defaultFloatPanelLocationOffset.X,
                                           HostWindow.Location.Y + _defaultFloatPanelLocationOffset.Y),
                WindowSize = _defaultFloatPanelSize,
            };
            hostedPanel.ParentContainer = floatPanel;

            FloatPanels.Add(floatPanel);
            CreateFloatPanel(floatPanel);
        }
    }

    /// <summary>
    /// Hide dock panel. Its location in the layout will be remembered.
    /// </summary>
    /// <param name="id">Target dock panel identificator</param>
    public void HidePanel(string id)
    {
        if (_dockingLayout.FindDockPanel(id, out var hostPanel) is DockPanelScheme panel)
        {
            panel.IsHidden = true;
            if (hostPanel is DockPanelFloatScheme floatPanel && floatPanel.ComputedIsHidden)
                floatPanel.Destroy();
        }
    }

    /// <summary>
    /// Gets dock panel hidden state.
    /// </summary>
    /// <param name="id">Target dock panel identificator</param>
    public bool IsPanelHidden(string id)
    {
        return _dockingLayout.FindDockPanel(id, out _)?.IsHidden ?? true;
    }

    /// <summary>
    /// Reset docking layout to default.
    /// </summary>
    public void ResetLayout()
    {
        foreach (var floatPanel in FloatPanels.Where(fp => !fp.ComputedIsHidden))
            floatPanel.Destroy();

        _dockingLayout = new();
        LayoutLoaded?.Invoke();
    }

    /// <summary>
    /// Save current docking layout to JSON
    /// </summary>
    public string SaveLayout()
    {
        return JsonSerializer.Serialize(_dockingLayout, _serializerOptions);
    }

    /// <summary>
    /// Load and apply docking layout from JSON.
    /// </summary>
    /// <param name="layoutJson">Layout JSON</param>
    /// <returns>Is layout deserialization was successful or not</returns>
    public bool LoadLayout(string layoutJson)
    {
        if (layoutJson is null)
            return false;

        DockingLayout newLayout;
        try
        {
            newLayout = JsonSerializer.Deserialize<DockingLayout>(layoutJson);
        }
        catch(JsonException)
        {
            return false;
        }

        foreach (var floatPanel in FloatPanels.Where(fp => !fp.ComputedIsHidden))
            floatPanel.Destroy();

        _dockingLayout = newLayout;

        foreach (var floatPanel in FloatPanels.Where(fp => !fp.ComputedIsHidden))
            CreateFloatPanel(floatPanel);

        LayoutLoaded?.Invoke();
        return true;
    }
}
