using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace Wired.Caching.Sample.Mvc.Autofac
{
    public static class AutofacBuilder
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            //Caching service
            builder.RegisterType<InMemoryCache>().As<ICacheService>().SingleInstance();
            
            var container = builder.Build();
            var mvcResolver = new AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(mvcResolver);
        }
    }
}
