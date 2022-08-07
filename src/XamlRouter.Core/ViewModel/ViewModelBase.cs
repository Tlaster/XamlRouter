namespace XamlRouter.Core.ViewModel;


public abstract class ViewModelBase : IDisposable
{
    protected internal ViewModelScope Scope { get; } = new();

    public virtual void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}