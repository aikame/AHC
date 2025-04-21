using Microsoft.AspNetCore.Mvc;

namespace DBC.Controllers
{
    public class ComputerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
