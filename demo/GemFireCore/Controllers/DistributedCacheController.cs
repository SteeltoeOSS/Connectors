using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GemFireCore.Controllers
{
    public class DistributedCacheController : Controller
    {
        private IDistributedCache _cache;
        private readonly Random rando = new Random();
        private readonly List<string> sampleData = new List<string> { "Artichokes", "Beans", "Beets", "Brussel Sprouts", "Cabbages", "Carrots", "Cucumbers", "Eggplants", "Leeks", "Onions", "Peppers", "Potatoes", "Pumpkins", "Radishes" };

        public DistributedCacheController(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IActionResult Index()
        {
            int hitCount = HttpContext.Session.GetInt32("HitCount") ?? 0;
            HttpContext.Session.SetInt32("HitCount", hitCount + 1);
            return View(hitCount);
        }

        public ActionResult GetCacheEntry()
        {
            ViewBag.Message = Encoding.ASCII.GetString(_cache.Get("BestVegetable") ?? Encoding.ASCII.GetBytes("Cache has not been set yet."));

            return View();
        }

        public ActionResult SetCacheEntry()
        {
            var bestfruit = sampleData.OrderBy(g => Guid.NewGuid()).First();

            _cache.Set("BestVegetable", Encoding.ASCII.GetBytes($"{bestfruit} are the best vegetables. Here's random id:{rando.Next()}"));

            return RedirectToAction("GetCacheEntry");
        }

        public ActionResult Reset()
        {
            HttpContext.Session.Clear();
            _cache.Remove("BestVegetable");

            return RedirectToAction("Index");
        }
    }
}