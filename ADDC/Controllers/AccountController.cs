using Microsoft.AspNetCore.Mvc;

namespace ADDC.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
