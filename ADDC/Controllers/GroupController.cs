using Microsoft.AspNetCore.Mvc;

namespace ADDC.Controllers
{
    public class GroupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
