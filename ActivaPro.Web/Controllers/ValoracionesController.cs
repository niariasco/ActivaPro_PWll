using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    [Authorize]
    public class ValoracionesController : Controller
    {
        private readonly IValoracionesService _service;
        private readonly ITicketesService _ticketService;

        public ValoracionesController(
            IValoracionesService service,
            ITicketesService ticketService)
        {
            _service = service;
            _ticketService = ticketService;
        }

        // ========== MÉTODOS AUXILIARES ==========

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue("id_usuario") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario autenticado");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue("rol") ?? User.FindFirstValue(ClaimTypes.Role) ?? "Cliente";
        }

        private string GetCurrentUserName()
        {
            return User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
        }

        // ========== INDEX - LISTADO DE VALORACIONES ==========

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;
            ViewBag.NombreUsuario = GetCurrentUserName();

            try
            {
                var valoraciones = await _service.ListByRolAsync(idUsuarioActual, rolUsuarioActual);

                // Obtener estadísticas según el rol
                ValoracionEstadisticasDTO estadisticas;
                if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
                {
                    estadisticas = await _service.GetEstadisticasByTecnicoAsync(idUsuarioActual);
                }
                else
                {
                    estadisticas = await _service.GetEstadisticasAsync();
                }

                ViewBag.Estadisticas = estadisticas;

                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View(valoraciones);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar las valoraciones: {ex.Message}";
                return View(Enumerable.Empty<ValoracionDTO>());
            }
        }

        // ========== DETAILS - DETALLE DE VALORACIÓN ==========

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                int idUsuarioActual = GetCurrentUserId();
                string rolUsuarioActual = GetCurrentUserRole();

                var valoracion = await _service.FindByIdAsync(id);

                if (valoracion == null)
                {
                    TempData["Error"] = $"La valoración con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar acceso según rol
                var ticket = await _ticketService.FindByIdAsync(valoracion.IdTicket);
                if (ticket == null)
                {
                    TempData["Error"] = "No se pudo cargar el ticket asociado.";
                    return RedirectToAction(nameof(Index));
                }

                bool tieneAcceso = rolUsuarioActual.ToLower() switch
                {
                    "administrador" or "coordinador" => true,
                    "técnico" or "tecnico" => ticket.IdUsuarioAsignado == idUsuarioActual,
                    "cliente" => ticket.IdUsuarioSolicitante == idUsuarioActual,
                    _ => false
                };

                if (!tieneAcceso)
                {
                    TempData["Error"] = "No tienes permiso para ver esta valoración.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();
                ViewBag.Ticket = ticket;

                return View(valoracion);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la valoración: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== CREATE - GET ==========

        [HttpGet]
        public async Task<IActionResult> Create(int idTicket)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            //  VALIDACIÓN: Solo clientes pueden crear valoraciones
            if (rolUsuarioActual.ToLower() != "cliente")
            {
                TempData["Error"] = "⛔ Solo los clientes pueden valorar tickets.";
                return RedirectToAction("Details", "Ticketes", new { id = idTicket });
            }

            try
            {
                // Validar que el cliente puede valorar el ticket
                var (esValido, mensaje) = await _service.ValidarCreacionValoracionAsync(idTicket, idUsuarioActual);

                if (!esValido)
                {
                    TempData["Error"] = $"❌ {mensaje}";
                    return RedirectToAction("Details", "Ticketes", new { id = idTicket });
                }

                var dto = await _service.PrepareCreateDTOAsync(idTicket, idUsuarioActual);
                var ticket = await _ticketService.FindByIdAsync(idTicket);

                ViewBag.Ticket = ticket;
                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error al preparar la valoración: {ex.Message}";
                return RedirectToAction("Details", "Ticketes", new { id = idTicket });
            }
        }

        // ========== CREATE - POST ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ValoracionCreateDTO dto)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // ✅ VALIDACIÓN: Solo clientes
            if (rolUsuarioActual.ToLower() != "cliente")
            {
                TempData["Error"] = "⛔ Solo los clientes pueden valorar tickets.";
                return RedirectToAction("Details", "Ticketes", new { id = dto.IdTicket });
            }

            if (!ModelState.IsValid)
            {
                var ticket = await _ticketService.FindByIdAsync(dto.IdTicket);
                ViewBag.Ticket = ticket;
                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();
                TempData["Error"] = "⚠️ Por favor corrija los errores del formulario.";
                return View(dto);
            }

            try
            {
                int idValoracion = await _service.CreateValoracionAsync(dto, idUsuarioActual);

                TempData["Success"] = $"✅ ¡Valoración registrada exitosamente! Gracias por tu feedback de {dto.Puntaje}/5 ⭐";
                return RedirectToAction("Details", "Ticketes", new { id = dto.IdTicket });
            }
            catch (InvalidOperationException ex)
            {
                var ticket = await _ticketService.FindByIdAsync(dto.IdTicket);
                ViewBag.Ticket = ticket;
                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();
                TempData["Error"] = $"❌ {ex.Message}";
                return View(dto);
            }
            catch (Exception ex)
            {
                var ticket = await _ticketService.FindByIdAsync(dto.IdTicket);
                ViewBag.Ticket = ticket;
                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();
                TempData["Error"] = $"❌ Error al registrar la valoración: {ex.Message}";
                return View(dto);
            }
        }

        // ========== ESTADÍSTICAS (OPCIONAL - Vista Separada) ==========

        [HttpGet]
        public async Task<IActionResult> Estadisticas()
        {
            string rolUsuarioActual = GetCurrentUserRole();

            // Solo administradores y técnicos pueden ver estadísticas
            if (rolUsuarioActual.ToLower() != "administrador" &&
                rolUsuarioActual.ToLower() != "coordinador" &&
                rolUsuarioActual.ToLower() != "técnico" &&
                rolUsuarioActual.ToLower() != "tecnico")
            {
                TempData["Error"] = "No tienes permiso para ver estadísticas.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int idUsuarioActual = GetCurrentUserId();
                ValoracionEstadisticasDTO estadisticas;

                if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
                {
                    estadisticas = await _service.GetEstadisticasByTecnicoAsync(idUsuarioActual);
                }
                else
                {
                    estadisticas = await _service.GetEstadisticasAsync();
                }

                ViewBag.RolUsuario = rolUsuarioActual;
                return View(estadisticas);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar estadísticas: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}