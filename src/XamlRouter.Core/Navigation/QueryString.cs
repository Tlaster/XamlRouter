namespace XamlRouter.Core.Navigation;

public class QueryString
{
    private string _raw;

    internal QueryString(string raw)
    {
        _raw = raw;
    }
    
    public string? this[string key]
    {
        get
        {
            var keyValue = _raw
                .Split('&')
                .FirstOrDefault(x => x.StartsWith(key + "="));
            return keyValue?.Split('=')[1];
        }
    }
    
    public string[] Keys => _raw
        .Split('&')
        .Select(x => x.Split('=')[0])
        .ToArray();
    
    public string[] Values => _raw
        .Split('&')
        .Select(x => x.Split('=')[1])
        .ToArray();
    
    public string[] All => _raw
        .Split('&');

    public string[] Get(string key)
    {
        var keyValue = _raw
            .Split('&')
            .Where(x => x.StartsWith(key + "="));
        return keyValue.Select(x => x.Split('=')[1]).ToArray();
    }
}