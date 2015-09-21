#Wired.Caching#

This is a package to simplify caching in your .Net projects. It started as a simple function but has become useful enough for me to drop into a library of it's own. 

##Usage##

1. Add a reference to this library (duh!) by either:

    a. Downloading this codebase, or, I recommend;

    b. Use the [Nuget package](https://www.nuget.org/packages/Wired.Caching) by using the Nuget Package Manager in Visual Studio or running this in the package manager console:

        Install-Package Wired.Caching

2. Create an instance of the cache service:

        //This can go anywhere, or preferably be injected
        var cacheService = new InMemoryCache();

2. Replace code where you need something cached. For example this:

        var zombies = context.People
            .Where(p => p.IsWalking && p.IsDead)
            .ToList();

    Would be replaced with something like this:

        var zombies = cacheService.Get(
            "zombies",
            () => context.People
                      .Where(p => p.IsWalking && p.IsDead)
                      .ToList(),
            600);

    Or you can use the slightly shorter lambda syntax:

        var zombies = cacheService.Get(
            "zombies",
            context.People
                .Where(p => p.IsWalking && p.IsDead)
                .ToList,
            600);


##Caveat##

There's always a [catch](http://shouldiblamecaching.com/) right? The main thing you need to be concerned about is when caching something that uses deferred execution. A database context is a good example here, hence why my example ends with `ToList()`. That materialises the query so you are caching the results and not an `IQueryable` interface. If you forget to do that, you will probably end up with errors telling you that your context has gone away or disconnected.