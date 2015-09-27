using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Wired.Caching.Mvc
{
    /// <summary>
    /// Apply this to your MVC action method to provide seamless caching.
    /// </summary>
    public class WiredCacheAttribute : ActionFilterAttribute
    {
        private const string CachePrefix = "Wired.Caching.Mvc.Cache";
        private const string KeyOnAllParameters = "*";
        private const string UserNameParameter = "wcUserName";

        private readonly int _duration;
        private bool _foundInCache;

        private readonly CachingConfigSection _configuration;

        /// <summary>
        /// Default constructor to provide the duration for caching.
        /// </summary>
        /// <param name="duration">The amount of seconds to cache your data</param>
        public WiredCacheAttribute(int duration)
        {
            _duration = duration;
            _configuration = ConfigurationManager.GetSection("wiredCaching") as CachingConfigSection;
        }

        /// <summary>
        /// A comma separated list of parameter names that determine if the item is already in the cache
        /// </summary>
        public string KeyOn { get; set; }

        /// <summary>
        /// Flag to set whether the cache is different per user
        /// </summary>
        public bool KeyOnUser { get; set; }

        private IDictionary<string, object> ActionParameters { get; set; }
        private string Controller { get; set; }
        private string Action { get; set; }

        /// <summary>
        /// This method is an implementation of System.Web.Mvc.IActionFilter.OnActionExecuting(System.Web.Mvc.ActionExecutingContext)
        /// and supports the ASP.NET MVC infrastructure. It is not intended to be used directly from your code.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            Action = filterContext.ActionDescriptor.ActionName;
            ActionParameters = filterContext.ActionParameters;

            ICacheService cacheService = new InMemoryCache();

            var cacheResult = cacheService.ReadFromCache<ActionResult>(GenerateKey);

            if (cacheResult == null)
            {
                _foundInCache = false;
                base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = cacheResult;
                _foundInCache = true;
            }
        }

        /// <summary>
        /// This method is an implementation of System.Web.Mvc.IActionFilter.OnActionExecuted(System.Web.Mvc.ActionExecutedContext)
        /// and supports the ASP.NET MVC infrastructure. It is not intended to be used directly from your code.
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!_foundInCache)
            {
                ICacheService cacheService = new InMemoryCache();
                cacheService.InsertIntoCache(GenerateKey, filterContext.Result, _duration);
            }
            base.OnActionExecuted(filterContext);
        }

        private string GenerateKey
        {
            get
            {
                var keyBuilder = new StringBuilder();
                keyBuilder.Append($"{CachePrefix}:{Controller}:{Action}");

                var alwaysKeyOnUser = _configuration?.AlwaysKeyOnUser;

                if (KeyOnUser || alwaysKeyOnUser.GetValueOrDefault())
                {
                    var name = HttpContext.Current?.User?.Identity?.Name;

                    if (!string.IsNullOrEmpty(name))
                    {
                        keyBuilder.Append($":{UserNameParameter}:{name}");
                    }
                }

                if (string.IsNullOrEmpty(KeyOn)) return keyBuilder.ToString();

                var keyParameters = KeyOn.Split(',').Select(s => s.Trim()).ToArray();

                //Order the parameters and filter the ones we are going to use
                var orderedParameters = ActionParameters
                    .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                    .Where(p => KeyOn == KeyOnAllParameters || keyParameters.Contains(p.Key));

                foreach (var item in orderedParameters)
                {
                    keyBuilder.Append($":{item.Key}={item.Value}");
                }

                return keyBuilder.ToString();
            }
        }

    }

}


