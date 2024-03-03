using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockPanelScheme : DockPanelBaseScheme, ICloneable
{
    public static Size MinSize { get; set; } = new Size(100, 100);

    public string Id { get; set; } = string.Empty;

    private bool _isHidden = false;
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            if (_isHidden == value)
                return;

            var isHiddenChain = GetParentsChain().Select(p => (p, p.ComputedIsHidden)).ToArray();
            var floatPanel = GetTopParent() as DockPanelFloatScheme;
            var floatVisiblePanelsCount = floatPanel?.GetAllDockPanelsInside().Count(p => !p.IsHidden);

            _isHidden = value;

            foreach(var (parent, oldIsHidden) in isHiddenChain)
            {
                if (parent == ParentContainer || parent.ComputedIsHidden != oldIsHidden)
                {
                    parent.NotifyPropertyChanged(nameof(IsHidden));

                    if (floatVisiblePanelsCount.HasValue &&
                        (floatVisiblePanelsCount.Value > 1 && value || floatVisiblePanelsCount.Value == 1 && !value))
                    {
                        floatPanel.NotifyPropertyChanged(nameof(IsHidden));
                    }
                    break;
                }
            }
        }
    }

    private bool _isDetachedGhost;
    [JsonIgnore]
    public bool IsDetachedGhost
    {
        get => _isDetachedGhost;
        set
        {
            _isDetachedGhost = value;
            NotifyPropertyChanged(nameof(IsDetachedGhost));
        }
    }

    [JsonIgnore]
    public override Size ComputedMinSize => MinSize;

    public Func<object> GetPanelContextFunc { get; set; }

    private object _panelContextToRestore;
    private bool _panelContextStored;

    public void StorePanelContext()
    {
        _panelContextStored = GetPanelContextFunc != null;
        _panelContextToRestore = GetPanelContextFunc?.Invoke();
    }

    public bool TryRestorePanelContext(out object context)
    {
        if (_panelContextStored)
        {
            context = _panelContextToRestore;
            _panelContextToRestore = null;
            _panelContextStored = false;
            return true;
        }
        else
        {
            context = null;
            return false;
        }
    }

    public override DockPanelScheme FindDockPanel(string id)
    {
        return Id == id ? this : null;
    }

    public override IEnumerable<DockPanelScheme> GetAllDockPanelsInside()
    {
        yield return this;
        yield break;
    }

    #region ICloneable
    public object Clone()
    {
        return new DockPanelScheme() {
            Id = Id,
            IsHidden = IsHidden,
            _panelContextStored = GetPanelContextFunc != null,
            _panelContextToRestore = GetPanelContextFunc?.Invoke(),
        };
    }
    #endregion
}
