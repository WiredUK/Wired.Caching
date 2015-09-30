#Wired.Caching#

This is a package to simplify caching in your .Net projects. It started as a simple function but has become useful enough for me to drop into a library of it's own. 

##Usage##

1. Add a reference to this library (duh!) by either:

    a. Downloading this codebase, or, I recommend;

    b. Use the [Nuget package](https://www.nuget.org/packages/Wired.Caching) by using the Nuget Package Manager in Visual Studio or running this in the package manager console:

        Install-Package Wired.Caching

2. Create an instance of the cache service:

```c#
    //This can go anywhere, or preferably be injected
    var cacheService = new InMemoryCache();
```

2. Replace code where you need something cached. For example this:

```c#
    var zombies = context.People
        .Where(p => p.IsWalking && p.IsDead)
        .ToList();
```

    Would be replaced with something like this:

```c#
    var zombies = cacheService.Get(
        "zombies",
        () => context.People
                  .Where(p => p.IsWalking && p.IsDead)
                  .ToList(),
        600);
```

    Or you can use the slightly shorter lambda syntax:

```c#
    var zombies = cacheService.Get(
        "zombies",
        context.People
            .Where(p => p.IsWalking && p.IsDead)
            .ToList,
        600);
```

##Caveat##

There's always a [catch](http://shouldiblamecaching.com/) right? The main thing you need to be concerned about is when caching something that uses deferred execution. A database context is a good example here, hence why my example ends with `ToList()`. That materialises the query so you are caching the results and not an `IQueryable` interface. If you forget to do that, you will probably end up with errors telling you that your context has gone away or disconnected.

#Wired.Caching.Mvc#

So now you want to use caching in your MVC project? Use this package to simplify that even further. Have you ever wanted to cache the entire return from an Action method? Well now you can by adding a simple attribute.

##Usage##

So here is an example (but very boring) action:

```c#
public ActionResult Index()
{ 
	return View();
}
```

So how do we make it cached? Just add the `WiredCache` attribute, like this:

```c#
[WiredCache(600)]
public ActionResult Index()
{ 
	return View();
}
```

And now that action will not be called more than once every 600 seconds. 

Can we get a bit more clever? Sure, what happens if your action has parameters and each variation of parameter has a different output that you want to cache? Simple, just use the `KeyOn` property. Either specify a comma seperated list of parameter names or use `*` to use them all. So lets say you have an action that has a single parameter that you don't care about, just do this:

```
[WiredCache(600, KeyOn = "id")]
public ActionResult GetProduct(int id, string ignoreThisParameter)
{ 
	return View();
}
```

Now what happens when inside your action method, the output is different depending on the user that is logged in, perhaps your `Index` action displays the users name and some specific content to them? That's easy too, just use the `KeyOnUser` property, like this:

```c#
[WiredCache(600, KeyOnUser = true)]
public ActionResult Index()
{ 
	return View();
}
```

But typing `KeyOnUser = true` every time is boring right? Wouldn't it be nice if you could set it once somewhere and forget? Easy! Just configure in your `web.config`. First add in a new config section:

```xml
<configSections>
    <section name="wiredCaching" type="Wired.Caching.Mvc.CachingConfigSection,Wired.Caching.Mvc"/>
</configSections>
```
Then somewhere else in your config, add this:

```xml
<wiredCaching alwaysKeyOnUser="true" />
```