using XamlRouter.Core.Navigation.Route;

namespace XamlRouter.Core.Navigation;

internal record RouteGraph(string InitialRoute, List<IRoute> Routes);