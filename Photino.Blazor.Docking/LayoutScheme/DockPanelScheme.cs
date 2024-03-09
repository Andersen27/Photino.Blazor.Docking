using System.Drawing;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Photino.Blazor.Docking.LayoutScheme;

internal sealed class DockPanelScheme : DockPanelBaseScheme, ICloneable
{
    private Dictionary<FieldInfo, object> _storedComponentState = null;

    public static Size MinSize { get; set; } = new Size(100, 100);

    public string Id { get; set; } = string.Empty;

    private bool _isHidden = false;
    public bool IsHidden
    {
        get => _isHidden;
        set => ChangeIsHiddenState(value);
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

    [JsonIgnore]
    public Func<object> GetComponentInstanceFunc { get; set; }

    private void ChangeIsHiddenState(bool value)
    {
        if (_isHidden != value)
        {
            _isHidden = value;
            foreach (var parent in GetParentsChain())
                parent.NotifyPropertyChanged(nameof(IsHidden));
        } 
    }

    public void StoreComponentState()
    {
        //#if DEBUG
        //if (_storedComponentState != null) throw new Exception();
        //#endif

        var component = GetComponentInstanceFunc();

        _storedComponentState = [];
        foreach (var field in component.GetType().GetRuntimeFields().Where(f => !f.IsStatic))
        {
            try { _storedComponentState[field] = field.GetValue(component); }
            catch { }
        }
    }

    public bool TryRestoreComponentState(object newComponent)
    {
        if (_storedComponentState is null)
            return false;

        foreach (var (field, value) in _storedComponentState)
        {
            try { field.SetValue(newComponent, value); }
            catch { }
        }

        _storedComponentState = null;
        return true;
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
        var newInstance = new DockPanelScheme() {
            Id = Id,
            IsHidden = IsHidden,
            GetComponentInstanceFunc = GetComponentInstanceFunc,
        };
        newInstance.StoreComponentState();

        return newInstance;
    }
    #endregion
}
