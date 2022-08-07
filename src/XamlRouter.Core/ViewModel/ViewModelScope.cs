namespace XamlRouter.Core.ViewModel;

public sealed class ViewModelScope : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        _disposables.ForEach(x => x.Dispose());
    }

    internal void Add(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }
}