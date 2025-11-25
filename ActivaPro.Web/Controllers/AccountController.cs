using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;

        public AccountController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var result = await _auth.LoginAsync(dto, ip);
            if (!result.ok)
            {
                ModelState.AddModelError(string.Empty, result.error);
                return View(dto);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, result.userId.ToString()),
                new Claim(ClaimTypes.Name, result.nombre),
                new Claim("rol", result.rol),
                new Claim("id_usuario", result.userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }   

        [HttpGet]
        public IActionResult Logout() => RedirectToAction("Index", "Home");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            var id = User.FindFirstValue("id_usuario");
            if (int.TryParse(id, out var usuarioId))
            {
                await _auth.LogoutAsync(usuarioId);
            }

            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterDTO());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _auth.RegisterAsync(dto);
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }
    }
}
