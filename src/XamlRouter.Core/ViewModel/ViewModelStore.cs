namespace XamlRouter.Core.ViewModel;

public class ViewModelStore
{
    private readonly Dictionary<string, ViewModelBase> _viewModels = new();
    
    public void Put<TViewModel>(string key, TViewModel viewModel)
        where TViewModel : ViewModelBase
    {
        _viewModels[key] = viewModel;
    }
    
    public TViewModel Get<TViewModel>(string key)
        where TViewModel : ViewModelBase
    {
        return (TViewModel)_viewModels[key];
    }
    
    public void Clear()
    {
        foreach (var (_, value) in _viewModels)
        {
            value.Dispose();
        }

        _viewModels.Clear();
    }
}