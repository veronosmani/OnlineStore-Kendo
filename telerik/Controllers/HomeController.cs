using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using telerik.Models;

namespace telerik.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(); // Can show a landing or dashboard page
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // (optional) if you had a demo/test page
        public IActionResult Kendo()
        {
            return View();
        }
    }
}
