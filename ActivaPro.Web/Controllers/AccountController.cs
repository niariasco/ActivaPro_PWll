using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;
        private readonly INotificacionService _notifs;

        public AccountController(IAuthService auth, INotificacionService notifs)
        {
            _auth = auth;
            _notifs = notifs;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginDTO());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
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
        public IActionResult Register() => View(new RegisterDTO());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var id = await _auth.RegisterAsync(dto);
                await _notifs.CrearEventoTicketAsync(new[] { id }, 0, "Registro", $"Usuario registrado: {dto.Nombre}", "Sistema");
                return RedirectToAction(nameof(Login));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // Olvidaste tu contraseña (público)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var ok = await _auth.ChangePasswordByEmailAsync(dto.Correo, dto.UltimaContrasena, dto.NuevaContrasena);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo cambiar la contraseña.");
                    return View(dto);
                }

                var usuario = await _auth.GetUsuarioInfoAsync(dto.Correo);
                if (usuario != null)
                {
                    await _notifs.CrearEventoTicketAsync(new[] { usuario.IdUsuario }, 0, "Seguridad", "Contraseña restablecida", usuario.Nombre);
                }

                TempData["Success"] = "Contraseña actualizada correctamente.";
                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error inesperado al actualizar la contraseña.");
                return View(dto);
            }
        }

        // Cambiar contraseña (solo autenticado)
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var idStr = User.FindFirstValue("id_usuario");
            if (!int.TryParse(idStr, out var id)) return RedirectToAction(nameof(Login));
            return View(new ChangePasswordDTO { IdUsuario = id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var ok = await _auth.ChangePasswordAsync(dto.IdUsuario, dto);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo cambiar la contraseña.";
                    return View(dto);
                }
                await _notifs.CrearEventoTicketAsync(new[] { dto.IdUsuario }, 0, "Seguridad", "Contraseña modificada", User.Identity?.Name ?? "Usuario");
                TempData["Success"] = "Contraseña cambiada.";
                return RedirectToAction(nameof(Profile));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch
            {
                TempData["Error"] = "Error inesperado al cambiar la contraseña.";
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var idStr = User.FindFirstValue("id_usuario");
            if (!int.TryParse(idStr, out var id)) return RedirectToAction(nameof(Login));
            var profile = await _auth.GetProfileAsync(id);
            if (profile == null) return NotFound();
            return View(profile);
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
    }
}