using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDTO());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(dto);

            var (ok, userId, nombre, rol, error) = await _auth.LoginAsync(
                dto,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP");

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim("id_usuario", userId.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Role, rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = dto.Recordarme,
                    ExpiresUtc = dto.Recordarme ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterDTO());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _auth.RegisterAsync(dto);
                TempData["Success"] = "Cuenta creada. Inicie sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var id = int.Parse(User.FindFirst("id_usuario")!.Value);
            var profile = await _auth.GetProfileAsync(id);
            return View(profile);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var ok = await _auth.UpdateProfileAsync(dto);
                if (ok) TempData["Success"] = "Perfil actualizado.";
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordDTO());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var id = int.Parse(User.FindFirst("id_usuario")!.Value);
            try
            {
                await _auth.ChangePasswordAsync(id, dto);
                TempData["Success"] = "Contraseña actualizada.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
