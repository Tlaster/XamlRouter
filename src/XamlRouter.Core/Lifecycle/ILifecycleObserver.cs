namespace XamlRouter.Core.Lifecycle;

public interface ILifecycleObserver
{
    void OnStateChanged(State state);
}