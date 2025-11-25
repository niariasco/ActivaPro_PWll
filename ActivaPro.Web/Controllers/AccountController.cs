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

        public AccountController(IAuthService auth) => _auth = auth;

        [HttpGet]
        public IActionResult Login() => View(new LoginDTO());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjax()) return BadRequest(new { ok = false, error = "Revisa los campos del formulario." });
                return View(dto);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var result = await _auth.LoginAsync(dto, ip);

            if (!result.ok)
            {
                if (IsAjax()) return BadRequest(new { ok = false, error = result.error });
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

            if (IsAjax()) return Json(new { ok = true, redirect = Url.Action("Index", "Home") });
            return RedirectToAction("Index", "Home");
        }   

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

        private bool IsAjax()
        {
            var xrw = Request.Headers["X-Requested-With"].ToString();
            var accept = Request.Headers["Accept"].ToString();
            return xrw == "XMLHttpRequest" || accept.Contains("application/json");
        }
    }
}
