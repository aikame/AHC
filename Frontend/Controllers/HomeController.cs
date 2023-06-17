using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IElasticClient elasticClient, IWebHostEnvironment hsEvironment, ILogger<HomeController> logger)
        {
            _elasticClient = elasticClient;
            _webHostEnvironment = hsEvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
    }
}