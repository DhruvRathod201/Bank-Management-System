using BankManagementSystem.DTOs;
using BankManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpGet]
        public IActionResult Login() =>
            User.Identity?.IsAuthenticated == true
                ? RedirectToAction("Index", "Home")
                : View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            // LoginAsync now handles cookie sign-in internally and returns (bool, string, string role)
            var (success, message, role) = await _auth.LoginAsync(dto, HttpContext);

            if (!success)
            {
                TempData["Error"] = message;
                return View(dto);
            }

            return role == "Admin"
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Dashboard", "Customer");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            // RegisterAsync returns (bool Success, string Message)
            var (success, message) = await _auth.RegisterAsync(dto);

            if (!success)
            {
                TempData["Error"] = message;
                return View(dto);
            }

            TempData["Success"] = message;
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync(HttpContext);
            return RedirectToAction("Login");
        }
    }
}
