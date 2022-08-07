using XamlRouter.Core.Navigation.Route;

namespace XamlRouter.Core.Navigation;

internal record RouteMatchResult(IRoute Route, Dictionary<string, string> PathMap);