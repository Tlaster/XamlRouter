using XamlRouter.Core.ViewModel;

namespace XamlRouter.Core.Navigation;

internal class NavControllerViewModel : ViewModelBase
{
    private readonly Dictionary<long, ViewModelStore> _viewModelStores = new();
    
    public ViewModelStore this[long key]
    {
        get
        {
            if (!_viewModelStores.TryGetValue(key, out var store))
            {
                store = new ViewModelStore();
                _viewModelStores.Add(key, store);
            }
            return store;
        }
    }
    
    public void Remove(long key)
    {
        _viewModelStores[key].Clear();
        _viewModelStores.Remove(key);
    }

    public override void Dispose()
    {
        foreach (var (_, value) in _viewModelStores)
        {
            value.Clear();
        }
        _viewModelStores.Clear();
        base.Dispose();
    }
}