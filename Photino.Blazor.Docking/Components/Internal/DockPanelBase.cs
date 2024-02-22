using Microsoft.AspNetCore.Components;
using Photino.Blazor.Docking.LayoutScheme;
using System;
using System.ComponentModel;

namespace Photino.Blazor.Docking.Components.Internal;

public abstract class DockPanelBase : ComponentBase, IDisposable
{
    private DockPanelBaseScheme _dockScheme;

    [CascadingParameter(Name = "DockScheme")]
    internal DockPanelBaseScheme DockScheme
    {
        get => _dockScheme;
        set
        {
            if (_dockScheme == value)
                return;

            var oldValue = _dockScheme;
            _dockScheme = value;
            DockSchemeChanged(oldValue, value);
        }
    }

    [CascadingParameter(Name = "ParentContainerScheme")]
    internal DockPanelContainerScheme ParentContainerScheme
    {
        set
        {
            if (DockScheme is null || DockScheme == value)
                return; //throw new InvalidOperationException();

            DockScheme.ParentContainer = value;
        }
    }

    [CascadingParameter(Name = "ParentComponent")]
    protected DockPanelContainer ParentComponent { get; set; }

    private void DockSchemeChanged(DockPanelBaseScheme oldScheme, DockPanelBaseScheme newScheme)
    {
        if (oldScheme != null)
            oldScheme.PropertyChanged -= OnDockSchemePropertyChanged;

        if (newScheme != null)
            newScheme.PropertyChanged += OnDockSchemePropertyChanged;
    }

    protected virtual void OnDockSchemePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected override void OnInitialized()
    {
        ParentComponent?.AddChildPanel(this);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && ParentComponent != null)
        {
            ParentComponent = null;
            StateHasChanged();
        }
    }

    public virtual void Dispose()
    {
        if (DockScheme != null)
            DockScheme.PropertyChanged -= OnDockSchemePropertyChanged;
    }
}
