using Apache.Geode.Client;
using Gemfire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gemfire.Controllers
{
    public class HomeController : Controller
    {
        private readonly Random rando = new Random();
        private static IRegion<string, string> cacheRegion;
        private readonly List<string> sampleData = new List<string> { "Apples", "Apricots", "Avacados", "Bananas", "Blueberries", "Lemons", "Limes", "Mangos", "Oranges", "Pears", "Pineapples" };
        private string _regionName = "SteeltoeDemo";

        public HomeController(PoolFactory poolFactory, Cache cache)
        {
            if (cacheRegion == null)
            {
                InitializeGemFireObjects(poolFactory, cache);
            }
        }

        public ActionResult Index()
        {
            var items = new List<string>() { "disabled" };

            //if (Session["items"] != null)
            //{
            //    if (Session["items"] is List<string>)
            //    {
            //        items = Session["items"] as List<string>;
            //        items.Add($"Random{rando.Next()}");
            //    }
            //}

            //Session["items"] = items;

            return View(items);
        }

        public ActionResult Reset()
        {
            // Session.Abandon();
            // Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            cacheRegion.Remove("BestFruit");
            return RedirectToAction("Index");
        }

        public ActionResult GetCacheEntry()
        {
            string message;
            try
            {
                message = cacheRegion["BestFruit"];
            }
            catch (RegionDestroyedException)
            {
                message = "The region SteeltoeDemo has not been initialized in Gemfire.\r\nConnect to Gemfire with gfsh and run 'create region --name=SteeltoeDemo --type=PARTITION'";
            }
            catch (Apache.Geode.Client.KeyNotFoundException)
            {
                message = "Cache has not been set yet.";
            }

            ViewBag.Message = message;

            return View();
        }

        public ActionResult SetCacheEntry()
        {
            var bestfruit = sampleData.OrderBy(g => Guid.NewGuid()).First();
            cacheRegion["BestFruit"] = $"{bestfruit} are the best fruit.";

            return RedirectToAction("GetCacheEntry");
        }

        private void InitializeGemFireObjects(PoolFactory poolFactory, Cache cache)
        {
            //var cacheFactory = new CacheFactory()
            //    .SetAuthInitialize(new BasicAuthInitialize("username", "password"));
            //gemfireCache = cacheFactory.Create();
            //var poolFactory = gemfireCache.GetPoolFactory().AddLocator("192.168.12.220", 55221);

            try
            {
                // make sure the pool has been created
                poolFactory.Create("pool");
            }
            catch (IllegalStateException e)
            {
                // we end up here with this message if you've hit the reset link after the pool was created
                if (e.Message != "Pool with the same name already exists")
                {
                    throw;
                }
            }

            var regionFactory = cache.CreateRegionFactory(RegionShortcut.PROXY).SetPoolName("pool");
            try
            {
                cacheRegion = regionFactory.Create<string, string>(_regionName);
            }
            catch
            {
                Console.WriteLine("Create CacheRegion failed... now trying to get the region");
                cacheRegion = cache.GetRegion<string, string>(_regionName);
            }
        }
    }
}