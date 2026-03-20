using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole("Admin")
                    ? RedirectToAction("Dashboard", "Admin")
                    : RedirectToAction("Dashboard", "Customer");
            }
            return View();
        }
    }
}
