using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Wired.Caching.Mvc
{
    public class WiredCacheAttribute : ActionFilterAttribute
    {
        private const string CachePrefix = "Wired.Caching.Mvc.Cache";
        private const string KeyOnAllParameters = "*";

        private readonly int _duration;
        private bool _foundInCache;

        public WiredCacheAttribute(int duration)
        {
            _duration = duration;
        }

        /// <summary>
        /// A comma separated list of parameter names that determine if the item is already in the cache
        /// </summary>
        public string KeyOn { get; set; }

        private IDictionary<string, object> ActionParameters { get; set; }
        private string Controller { get; set; }
        private string Action { get; set; }

        private string GenerateKey
        {
            get
            {
                var keyBuilder = new StringBuilder();
                keyBuilder.AppendFormat("{0}:{1}:{2}", CachePrefix, Controller, Action);

                if (string.IsNullOrEmpty(KeyOn)) return keyBuilder.ToString();

                var keyParameters = KeyOn.Split(',').Select(s => s.Trim()).ToArray();

                //Order the parameters and filter the ones we are going to use
                var orderedParameters = ActionParameters
                    .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                    .Where(p => KeyOn == KeyOnAllParameters || keyParameters.Contains(p.Key));

                foreach (var item in orderedParameters)
                {
                    keyBuilder.AppendFormat(":{0}={1}", item.Key, item.Value);
                }

                return keyBuilder.ToString();
            }
        }

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

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!_foundInCache)
            {
                ICacheService cacheService = new InMemoryCache();
                cacheService.InsertIntoCache(GenerateKey, filterContext.Result, _duration);
            }
            base.OnActionExecuted(filterContext);
        }


    }



}


