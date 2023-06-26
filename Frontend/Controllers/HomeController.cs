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
                var arg = new Dictionary<string, string>();
                arg.Add("name", user.Name);
                arg.Add("surname", user.SurName);
                arg.Add("midname", user.MidName);
                arg.Add("city", user.City);
                arg.Add("company", user.Company);
                arg.Add("departement", user.Department);
                arg.Add("position", user.Appointment);

                string result = (await HttpClass.GetInstance().Post("CreateUser", arg)).ToString();
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