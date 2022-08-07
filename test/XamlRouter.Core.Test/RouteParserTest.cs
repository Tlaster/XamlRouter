using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XamlRouter.Core.Test;

[TestClass]
public class RouteParserTest
{
    [TestMethod]
    public void PathKeys()
    {
        PathKeys("/{lang:[a-z]{2}}", keys => CollectionAssert.AreEqual(new [] {"lang"}, keys));
        PathKeys("/edit/{id}?", keys => CollectionAssert.AreEqual(new [] {"id"}, keys) );
        PathKeys("/path/{id}/{start}?/{end}?", keys => CollectionAssert.AreEqual(new [] {"id", "start", "end"}, keys) );
        PathKeys("/*", keys => Assert.AreEqual(1, keys.Count) );
        PathKeys("/foo/?*", keys => Assert.AreEqual(1, keys.Count) );
        PathKeys("/foo", keys => Assert.AreEqual(0, keys.Count) );
        PathKeys("/", keys => Assert.AreEqual(0, keys.Count) );
        PathKeys("/foo/bar", keys => Assert.AreEqual(0, keys.Count) );
        PathKeys("/foo/*", keys => Assert.AreEqual(1, keys.Count) );
        PathKeys("/foo/*name", keys => Assert.AreEqual(1, keys.Count) );
        PathKeys("/foo/{x}", keys => Assert.AreEqual(1, keys.Count) );
        PathKeys("aaa://{lang:[a-z]{2}}", keys => CollectionAssert.AreEqual(new [] {"lang"}, keys) );
        PathKeys("bbb://path/{id}/{start}?/{end}?", keys => CollectionAssert.AreEqual(new [] {"id", "start", "end"}, keys) );
    }

    [TestMethod]
    public void PathKeyMap()
    {
        PathKeyMap("/{lang:[a-z]{2}}", map => Assert.AreEqual("[a-z]{2}", map["lang"]) );
        PathKeyMap("/{id:[0-9]+}", map => Assert.AreEqual("[0-9]+", map["id"]) );
        PathKeyMap("/edit/{id}?", keys => Assert.AreEqual(null, keys["id"]) );
        PathKeyMap("/path/{id}/{start}?/{end}?",
            keys =>
            {
                Assert.AreEqual(null, keys["id"]);
                Assert.AreEqual(null, keys["start"]);
                Assert.AreEqual(null, keys["end"]);
            });
        PathKeyMap("/*", keys => Assert.AreEqual("\\.*", keys["*"]) );
        PathKeyMap("/foo/?*", keys => Assert.AreEqual("\\.*", keys["*"]) );
        PathKeyMap("/foo/*name", keys => Assert.AreEqual("\\.*", keys["name"]) );

        PathKeyMap("aaa://foo/?*", keys => Assert.AreEqual("\\.*", keys["*"]) );
        PathKeyMap("bbb://foo/*name", keys => Assert.AreEqual("\\.*", keys["name"]) );
    }
    
    [TestMethod]
    public void WildOnRoot()
    {
        var parser = new RouteParser();

        parser.Insert(Route("/foo/?*", "foo"));
        parser.Insert(Route("/bar/*", "bar"));
        parser.Insert(Route("/*", "root"));
        
        FindAssert(parser, "/", "root");
        FindAssert(parser, "/foo", "foo");
        FindAssert(parser, "/bar", "root");
        FindAssert(parser, "/foox", "root");
        FindAssert(parser, "/foo/", "foo");
        FindAssert(parser, "/foo/x", "foo");
        FindAssert(parser, "/bar/x", "bar");
    }

    [TestMethod]
    public void SearchString()
    {
        var parser = new RouteParser();
        parser.Insert(Route("/regex/{nid:[0-9]+}", "nid"));
        parser.Insert(Route("/regex/{zid:[0-9]+}/edit", "zid"));
        parser.Insert(Route("/articles/{id}", "id"));
        parser.Insert(Route("/articles/*", "*"));
        
        FindAssert(parser, "/regex/678/edit", "zid");
        FindAssert(parser, "/articles/tail/match", "*");
        FindAssert(parser, "/articles/123", "id");
    }

    [TestMethod]
    public void SearchParam()
    {
        var parser = new RouteParser();
        parser.Insert(Route("/articles/{id}", "id"));
        parser.Insert(Route("/articles/*", "catchall"));
        
        FindAssert(parser, "/articles/123", "id");
        FindAssert(parser, "/articles/tail/match", "catchall");
    }

    [TestMethod]
    public void MultipleRegex()
    {
        var parser = new RouteParser();
        parser.Insert(Route("/{lang:[a-z][a-z]}/{page:[^.]+}/", "1515"));
        
        Assert.IsNull(parser.Find("/12/f/"));
        FindAssert(parser, "/ar/page/", "1515");
        Assert.IsNull(parser.Find("/arx/page/"));
    }

    [TestMethod]
    public void RegexWithQuantity()
    {
        var parser = new RouteParser();
        parser.Insert(Route("/{lang:[a-z]{2}}/", "qx"));
        
        Assert.IsNull(parser.Find("/12/"));
        FindAssert(parser, "/ar/", "qx");
    }

    [TestMethod]
    public void WithSchema()
    {
        var parser = new RouteParser();
        parser.Insert(Route("aaa://home", "1"));
        parser.Insert(Route("bbb://home", "2"));
        
        FindAssert(parser, "aaa://home", "1");
        FindAssert(parser, "bbb://home", "2");
    }

    [TestMethod]
    public void WithSchemaAndRegex()
    {
        var parser = new RouteParser();
        parser.Insert(Route("aaa://home", "1"));
        parser.Insert(Route("bbb://home", "2"));
        parser.Insert(Route("aaa://home/{id:[0-9]+}", "3"));
        
        FindAssert(parser, "aaa://home", "1");
        FindAssert(parser, "bbb://home", "2");
        FindAssert(parser, "aaa://home/123", "3");
    }

    [TestMethod]
    public void ShouldExpandOptionalParams()
    {
        RouteParser.ExpandOptionalVariables("/{lang:[a-z]{2}}?").Let(paths =>
        {
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/", paths.ElementAt(0));
            Assert.AreEqual("/{lang:[a-z]{2}}", paths.ElementAt(1));
        });
        RouteParser.ExpandOptionalVariables("/{lang:[a-z]{2}}").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/{lang:[a-z]{2}}", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/edit/{id:[0-9]+}?").Let(paths =>
        {
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/edit", paths.ElementAt(0));
            Assert.AreEqual("/edit/{id:[0-9]+}", paths.ElementAt(1));
        });
        RouteParser.ExpandOptionalVariables("/path/{id}/{start}?/{end}?").Let(paths =>
        {
            Assert.AreEqual(3, paths.Count);
            Assert.AreEqual("/path/{id}", paths.ElementAt(0));
            Assert.AreEqual("/path/{id}/{start}", paths.ElementAt(1));
            Assert.AreEqual("/path/{id}/{start}/{end}", paths.ElementAt(2));
        });
        RouteParser.ExpandOptionalVariables("/{id}?/suffix").Let(paths =>
        {
            Assert.AreEqual(3, paths.Count);
            Assert.AreEqual("/", paths.ElementAt(0));
            Assert.AreEqual("/{id}/suffix", paths.ElementAt(1));
            Assert.AreEqual("/suffix", paths.ElementAt(2));
        });
        RouteParser.ExpandOptionalVariables("/prefix/{id}?").Let(paths =>
        {
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/prefix", paths.ElementAt(0));
            Assert.AreEqual("/prefix/{id}", paths.ElementAt(1));
        });
        RouteParser.ExpandOptionalVariables("/{id}?").Let(paths =>
        {
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/", paths.ElementAt(0));
            Assert.AreEqual("/{id}", paths.ElementAt(1));
        });
        RouteParser.ExpandOptionalVariables("/path").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/path", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/path/subpath").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/path/subpath", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/{id}").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/{id}", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/{id}/suffix").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/{id}/suffix", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/prefix/{id}").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/prefix/{id}", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("/").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/", paths.ElementAt(0));
        });
        RouteParser.ExpandOptionalVariables("").Let(paths =>
        {
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("/", paths.ElementAt(0));
        });
    }
    
    private void PathKeys(string pattern, Action<List<string>> consumer)
    {
        consumer.Invoke(RouteParser.PathKeys(pattern));
    }
    
    private void PathKeyMap(string pattern, Action<Dictionary<string, string?>> consumer)
    {
        var map = new Dictionary<string, string?>();
        RouteParser.PathKeys(pattern, (key, value) => map[key] = value);
        consumer.Invoke(map);
    }
    
    private static void FindAssert(RouteParser parser, string path, string id)
    {
        var find = parser.Find(path);
        Assert.IsNotNull(find);
        Assert.IsInstanceOfType(find.Route, typeof(TestRoute));
        var route = (TestRoute)find.Route;
        Assert.AreEqual(id, route.Id);
    }
    
    private static Route Route(string path, string id)
    {
        return new TestRoute(path, id, new List<string>());
    }
}

record TestRoute(string Route, string Id, List<string> PathKeys) : Route;

internal static class LetExtension
{
    public static T Let<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}