namespace XamlRouter.Core.Lifecycle;

public interface ILifecycleOwner
{
    ILifecycle Lifecycle { get; }
}