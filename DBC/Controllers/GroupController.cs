using Microsoft.AspNetCore.Mvc;

namespace DBC.Controllers
{
    public class GroupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
