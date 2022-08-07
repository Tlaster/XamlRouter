using System.Collections.Immutable;
using XamlRouter.Core.Lifecycle;
using XamlRouter.Core.Navigation.Route;
using XamlRouter.Core.ViewModel;

namespace XamlRouter.Core.Navigation;

public class BackStackEntry : IViewModelStoreOwner, ILifecycleOwner
{
    private readonly LifecycleRegistry _lifecycleRegistry = new();
    private bool _destroyAfterNavigation;
    public long Id { get; }
    public IRoute Route { get; }
    public ImmutableDictionary<string, string> PathMap { get; }
    public QueryString? QueryString { get; }
    internal NavControllerViewModel ViewModel { get; }

    internal BackStackEntry(long id, IRoute route, ImmutableDictionary<string, string> pathMap, QueryString? queryString, NavControllerViewModel viewModel)
    {
        Id = id;
        Route = route;
        PathMap = pathMap;
        QueryString = queryString;
        ViewModel = viewModel;
    }

    public void Active()
    {
        _lifecycleRegistry.CurrentState = State.Active;
    }
    
    public void InActive()
    {
        _lifecycleRegistry.CurrentState = State.InActive;
        if (_destroyAfterNavigation)
        {
            Destroy();
        }
    }
    
    public void Destroy()
    {
        if (_lifecycleRegistry.CurrentState != State.InActive)
        {
            _destroyAfterNavigation = true;
        }
        else
        {
            _lifecycleRegistry.CurrentState = State.Destroyed;
        }
    }
    
    public T? Path<T>(string key, T? defaultValue = null) where T : class
    {
        if (PathMap.TryGetValue(key, out var value))
        {
            return Convert.ChangeType(value, typeof(T)) as T;
        }
        return defaultValue;
    }
    
    public T? Query<T>(string key, T? defaultValue = null) where T : class
    {
        if (QueryString is null)
        {
            return defaultValue;
        }

        var value = QueryString[key];
        if (value is null)
        {
            return defaultValue;
        }
        else
        {
            return Convert.ChangeType(value, typeof(T)) as T;
        }
    }

    public List<T> QueryList<T>(string key) where T : class
    {
        if (QueryString is null)
        {
            return new List<T>();
        }
        var value = QueryString.Get(key);
        return value.Select(it => Convert.ChangeType(it, typeof(T)) as T)
            .Where(it => it != null)
            .Select(it => it!)
            .ToList();
    }

    public ViewModelStore ViewModelStore => ViewModel[Id];
    public ILifecycle Lifecycle => _lifecycleRegistry;
}