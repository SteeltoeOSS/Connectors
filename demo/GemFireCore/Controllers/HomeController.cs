using Apache.Geode.Client;
using GemFire.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GemFire.Controllers
{
    public class HomeController : Controller
    {
        private readonly Random rando = new Random();

        private static IRegion<string, string> cacheRegion;
        private readonly List<string> sampleData = new List<string> { "Apples", "Apricots", "Avacados", "Bananas", "Blueberries", "Lemons", "Limes", "Mangos", "Oranges", "Pears", "Pineapples" };
        private readonly string _regionName = "SteeltoeDemo";

        public HomeController(PoolFactory poolFactory, Cache cache)
        {
            if (cacheRegion == null)
            {
                InitializeGemFireObjects(poolFactory, cache);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult GetCacheEntry()
        {
            string message;
            try
            {
                message = cacheRegion["BestFruit"];
            }
            // catch (RegionDestroyedException) not sure why this isn't being thrown anymore ... ?
            catch (CacheServerException)
            {
                message = "The region SteeltoeDemo has not been initialized in GemFire.\r\nConnect to GemFire with gfsh and run 'create region --name=SteeltoeDemo --type=PARTITION'";
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
            cacheRegion["BestFruit"] = $"{bestfruit} are the best fruit. Here's random id:{rando.Next()}";

            return RedirectToAction("GetCacheEntry");
        }

        public ActionResult Reset()
        {
            //Session.Abandon();
            //Request.Cookies.Clear();
            //Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            cacheRegion.Remove("BestFruit");
            cacheRegion = null;
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void InitializeGemFireObjects(PoolFactory poolFactory, Cache cache)
        {
            //var cacheFactory = new CacheFactory().SetPdxIgnoreUnreadFields(true)
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
