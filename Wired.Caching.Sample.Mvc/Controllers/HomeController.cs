using System;
using System.Net.Http;
using System.Web.Mvc;
using Wired.Caching.Mvc;

namespace Wired.Caching.Sample.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheService _cacheService;

        public HomeController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [WiredCache(600, KeyOn = "test")]
        public ActionResult Index(string test = "blah")
        {
            return View();
        }

        public ActionResult InjectionDemo()
        {
            //Use the injected cache service
            var siteContent = _cacheService.Get(
                "SiteContent",
                GetDataFromWebsite,
                600);

            ViewBag.SiteContent = siteContent;
            return View();
        }

        [WiredCache(600)]
        public ActionResult AttributeDemo1()
        {
            var siteContent = GetDataFromWebsite();
            ViewBag.SiteContent = siteContent;
            return View("AttributeDemo");
        }

        [WiredCache(600, KeyOn = "id")]
        public ActionResult AttributeDemo2(string id, string someOtherParameter)
        {
            var siteContent = GetDataFromWebsite();
            ViewBag.SiteContent = siteContent;
            return View("AttributeDemo");
        }

        private static string GetDataFromWebsite()
        {
            var client = new HttpClient();

            var siteContentTask = client.GetStringAsync("http://www.google.com");
            siteContentTask.Wait();

            var content = $"Content downloaded on {DateTime.Now.ToString("hh:mm:ss")}\n\n{siteContentTask.Result}";

            //An arbitrary 5 second wait to slow things down
            System.Threading.Thread.Sleep(5000);
            return content;
        }
       
    }
}