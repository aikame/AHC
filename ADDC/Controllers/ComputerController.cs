using Microsoft.AspNetCore.Mvc;

namespace ADDC.Controllers
{
    public class ComputerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
