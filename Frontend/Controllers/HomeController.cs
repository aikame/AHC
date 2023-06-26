using Frontend.Context;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;
        private readonly AHCContext _context;

        public HomeController(IWebHostEnvironment hsEvironment, ILogger<HomeController> logger, AHCContext context)
        {
            _webHostEnvironment = hsEvironment;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCity(CitiesModel city)
        {
            return View();
        }

        public IActionResult Index()
        {
            ViewData["dbContext"] = _context;
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["dbContext"] = _context;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}