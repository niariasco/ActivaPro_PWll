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
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al Home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginDTO());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjax())
                    return BadRequest(new { ok = false, error = "Revisa los campos del formulario." });
                return View(dto);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var result = await _auth.LoginAsync(dto, ip);

            if (!result.ok)
            {
                if (IsAjax())
                    return BadRequest(new { ok = false, error = result.error });

                ModelState.AddModelError(string.Empty, result.error);
                return View(dto);
            }

            // Crear claims para la sesión - CORREGIDO
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, result.userId.ToString()),
                new Claim(ClaimTypes.Name, result.nombre),
                new Claim(ClaimTypes.Role, result.rol), // ✅ IMPORTANTE: Usar ClaimTypes.Role
                new Claim("rol", result.rol),            // ✅ Mantener también el claim personalizado
                new Claim("id_usuario", result.userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = dto.Recordarme, // Si marcó "Recordarme"
                ExpiresUtc = dto.Recordarme
                    ? System.DateTimeOffset.UtcNow.AddDays(30)
                    : System.DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            if (IsAjax())
                return Json(new { ok = true, redirect = Url.Action("Index", "Home") });

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register()
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterDTO());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {

                int userId = await _auth.RegisterAsync(dto, "Cliente");


                TempData["Success"] = "✓ Cuenta creada exitosamente. Ahora puedes iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (System.InvalidOperationException ex)
            {
                // Usuario ya existe
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (System.Exception ex)
            {
                // Error general
                ModelState.AddModelError(string.Empty, $"Error al crear la cuenta: {ex.Message}");
                return View(dto);
            }
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

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var id = User.FindFirstValue("id_usuario");
            if (int.TryParse(id, out var usuarioId))
            {
                await _auth.LogoutAsync(usuarioId);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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