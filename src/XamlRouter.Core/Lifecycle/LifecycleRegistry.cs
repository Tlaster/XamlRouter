using System.Collections.Concurrent;

namespace XamlRouter.Core.Lifecycle;

class LifecycleRegistry : ILifecycle
{
    private readonly ConcurrentBag<ILifecycleObserver> _observers = new();
    private State _currentState = State.Initialized;

    public State CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == State.Destroyed || value == State.Initialized)
            {
                return;
            }
            _currentState = value;
            DispatchState(value);
        }
    }

    private void DispatchState(State value)
    {
        foreach (var observer in _observers)
        {
            observer.OnStateChanged(value);
        }
    }

    public bool HasObservers => _observers.IsEmpty;
    public void AddObserver(ILifecycleObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(ILifecycleObserver observer)
    {
        _observers.TryTake(out _);
    }
}