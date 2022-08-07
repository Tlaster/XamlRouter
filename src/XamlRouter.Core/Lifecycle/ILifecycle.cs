namespace XamlRouter.Core.Lifecycle;

public interface ILifecycle
{
    State CurrentState { get; }
    bool HasObservers { get; }
    void AddObserver(ILifecycleObserver observer);
    void RemoveObserver(ILifecycleObserver observer);
}