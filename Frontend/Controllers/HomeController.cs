using Frontend.Classes;
using Frontend.Context;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

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
        public async Task<IActionResult> AddUser(UserModel user)
        {
            if (user != null) 
            {
              

                string result = (await HttpClass.GetInstance().Post("CreateUser", user)).ToString();
                Console.WriteLine(result);
            }

            return Redirect("/");
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