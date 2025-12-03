using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    [Authorize]
    public class AsignacionesController : Controller
    {
        private readonly IAsignacionesService _service;
        private readonly ITicketesService _ticketService;

        public AsignacionesController(
            IAsignacionesService service,
            ITicketesService ticketService)
        {
            _service = service;
            _ticketService = ticketService;
        }

        // ========== MÉTODOS AUXILIARES ==========

        /// <summary>
        /// Obtiene el ID del usuario autenticado desde los Claims
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue("id_usuario") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario autenticado");
        }

        /// <summary>
        /// Obtiene el rol del usuario autenticado desde los Claims
        /// </summary>
        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("rol") ?? "Cliente";
        }

        // ========== VISTAS PRINCIPALES ==========

        /// <summary>
        /// Vista principal del tablero de asignaciones
        /// Muestra todos los técnicos con sus tickets asignados organizados por semana
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var tecnicos = await _service.GetAsignacionesPorTecnicoAsync();

                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View(tecnicos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar asignaciones: {ex.Message}";
                return View(new List<TecnicoAsignacionesDTO>());
            }
        }

        /// <summary>
        /// Vista de asignaciones específicas de un técnico
        /// </summary>
        /// <param name="id">ID del técnico (opcional, usa el ID del usuario actual si no se proporciona)</param>
        public async Task<IActionResult> MisAsignaciones(int? id)
        {
            try
            {
                int idTecnico = id ?? GetCurrentUserId();
                var tecnico = await _service.GetAsignacionesByTecnicoIdAsync(idTecnico);

                if (tecnico == null)
                {
                    TempData["Error"] = "No se encontraron asignaciones para este técnico.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IdTecnico = idTecnico;
                return View(tecnico);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar asignaciones: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Redirige a la vista de detalles del ticket
        /// </summary>
        /// <param name="id">ID del ticket</param>
        public async Task<IActionResult> DetalleTicket(int id)
        {
            try
            {
                var ticket = await _ticketService.FindByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = "El ticket no existe.";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Ticketes/Details.cshtml", ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== ASIGNACIÓN AUTOMÁTICA (AUTOTRIAGE) ==========

        /// <summary>
        /// Vista para asignación automática de tickets
        /// Muestra todos los tickets pendientes y permite asignarlos automáticamente
        /// Solo accesible para administradores
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignacionAutomatica()
        {
            try
            {
                var ticketsPendientes = await _service.GetTicketsPendientesAsync();
                ViewBag.TotalTicketsPendientes = ticketsPendientes.Count();

                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View(ticketsPendientes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar tickets pendientes: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Asigna un ticket específico automáticamente usando el sistema de autotriage
        /// El sistema calcula el mejor técnico basándose en:
        /// - Prioridad del ticket
        /// - Tiempo restante del SLA
        /// - Carga de trabajo del técnico
        /// - Especialidades (si aplica)
        /// </summary>
        /// <param name="idTicket">ID del ticket a asignar</param>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTicketAutomaticamente(int idTicket)
        {
            try
            {
                var resultado = await _service.AsignarAutomaticamenteAsync(idTicket);

                if (resultado.Exitoso)
                {
                    TempData["Success"] = $"✓ {resultado.Mensaje} Ticket asignado a {resultado.TecnicoSeleccionado.NombreTecnico} con puntaje {resultado.Puntaje:F2}";
                }
                else
                {
                    TempData["Error"] = $"✗ {resultado.Mensaje}";
                }

                return RedirectToAction(nameof(AsignacionAutomatica));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"✗ Error al asignar ticket: {ex.Message}";
                return RedirectToAction(nameof(AsignacionAutomatica));
            }
        }

        /// <summary>
        /// Asigna automáticamente todos los tickets pendientes de una sola vez
        /// Usa el algoritmo de autotriage para cada ticket
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTodosPendientes()
        {
            try
            {
                var resultados = await _service.AsignarTodosPendientesAsync();
                var exitosos = resultados.Count(r => r.Exitoso);
                var fallidos = resultados.Count(r => !r.Exitoso);

                if (exitosos > 0)
                {
                    TempData["Success"] = $"✓ Se asignaron automáticamente {exitosos} tickets.";
                }

                if (fallidos > 0)
                {
                    TempData["Error"] = $"✗ {fallidos} tickets no pudieron ser asignados.";
                }

                return RedirectToAction(nameof(AsignacionAutomatica));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"✗ Error al asignar tickets: {ex.Message}";
                return RedirectToAction(nameof(AsignacionAutomatica));
            }
        }

        // ========== ASIGNACIÓN MANUAL ==========

        /// <summary>
        /// Vista para asignación manual de tickets
        /// Permite al administrador seleccionar manualmente el técnico para cada ticket
        /// Muestra información sobre la carga de trabajo y disponibilidad de cada técnico
        /// Solo accesible para administradores
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignacionManual()
        {
            try
            {
                var ticketsPendientes = await _service.GetTicketsPendientesAsync();
                var tecnicosDisponibles = await _service.GetTecnicosDisponiblesAsync();

                ViewBag.TicketsPendientes = ticketsPendientes?.ToList() ?? new List<TicketPendienteAsignacionDTO>();
                ViewBag.TecnicosDisponibles = tecnicosDisponibles?.ToList() ?? new List<TecnicoDisponibleDTO>();
                ViewBag.IdUsuarioActual = GetCurrentUserId();

                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar datos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la asignación manual de un ticket a un técnico específico
        /// </summary>
        /// <param name="idTicket">ID del ticket a asignar</param>
        /// <param name="idTecnico">ID del técnico al que se asignará el ticket</param>
        /// <param name="justificacion">Justificación opcional para la asignación manual</param>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTicketManualmente(int idTicket, int idTecnico, string justificacion)
        {
            try
            {
                int idUsuarioActual = GetCurrentUserId();

                var request = new AsignacionManualRequestDTO
                {
                    IdTicket = idTicket,
                    IdTecnico = idTecnico,
                    IdUsuarioAsignador = idUsuarioActual,
                    Justificacion = justificacion ?? "Asignación manual sin justificación específica"
                };

                var resultado = await _service.AsignarManualmenteAsync(request);

                if (resultado.Exitoso)
                {
                    TempData["Success"] = $"✓ {resultado.Mensaje} Ticket asignado a {resultado.TecnicoSeleccionado.NombreTecnico}";
                }
                else
                {
                    TempData["Error"] = $"✗ {resultado.Mensaje}";
                }

                return RedirectToAction(nameof(AsignacionManual));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"✗ Error al asignar ticket: {ex.Message}";
                return RedirectToAction(nameof(AsignacionManual));
            }
        }

        // ========== API ENDPOINTS ==========

        /// <summary>
        /// API endpoint que devuelve la lista de técnicos disponibles en formato JSON
        /// Opcionalmente puede recibir el ID de un ticket para filtrar técnicos
        /// con especialidades relacionadas
        /// </summary>
        /// <param name="idTicket">ID del ticket (opcional)</param>
        /// <returns>JSON con la lista de técnicos disponibles</returns>
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<JsonResult> GetTecnicosDisponibles(int? idTicket = null)
        {
            try
            {
                var tecnicos = await _service.GetTecnicosDisponiblesAsync(idTicket);
                return Json(new { success = true, data = tecnicos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API endpoint que devuelve los tickets pendientes de asignación en formato JSON
        /// </summary>
        /// <returns>JSON con la lista de tickets pendientes</returns>
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<JsonResult> GetTicketsPendientes()
        {
            try
            {
                var tickets = await _service.GetTicketsPendientesAsync();
                return Json(new { success = true, data = tickets });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== VISTAS ADICIONALES ==========

        /// <summary>
        /// Vista de detalles de una asignación específica
        /// Muestra información detallada sobre la asignación, incluyendo:
        /// - Puntaje (si es automática)
        /// - Justificación
        /// - Usuario que asignó
        /// - Fecha de asignación
        /// TODO: Implementar vista completa
        /// </summary>
        /// <param name="id">ID de la asignación</param>
        public async Task<IActionResult> DetalleAsignacion(int id)
        {
            // Esta vista puede implementarse para mostrar detalles específicos
            // de una asignación, incluyendo puntaje, justificación, etc.
            TempData["Info"] = "Vista de detalles de asignación (por implementar)";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Vista de estadísticas y reportes de asignaciones
        /// TODO: Implementar vista de reportes
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Reportes()
        {
            TempData["Info"] = "Vista de reportes (por implementar)";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Reasigna un ticket a otro técnico
        /// TODO: Implementar funcionalidad de reasignación
        /// </summary>
        /// <param name="idTicket">ID del ticket</param>
        /// <param name="nuevoIdTecnico">ID del nuevo técnico</param>
        /// <param name="justificacion">Justificación de la reasignación</param>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReasignarTicket(int idTicket, int nuevoIdTecnico, string justificacion)
        {
            TempData["Info"] = "Funcionalidad de reasignación (por implementar)";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Cancela una asignación existente y devuelve el ticket a estado Pendiente
        /// TODO: Implementar funcionalidad de cancelación
        /// </summary>
        /// <param name="idAsignacion">ID de la asignación a cancelar</param>
        /// <param name="motivo">Motivo de cancelación</param>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAsignacion(int idAsignacion, string motivo)
        {
            TempData["Info"] = "Funcionalidad de cancelación de asignación (por implementar)";
            return RedirectToAction(nameof(Index));
        }

        // ========== VISTAS AUXILIARES ==========

        /// <summary>
        /// Vista de configuración del sistema de asignación automática
        /// Permite configurar parámetros como:
        /// - Pesos de cada factor (prioridad, SLA, carga)
        /// - Límite de tickets por técnico
        /// - Reglas de especialización
        /// TODO: Implementar vista de configuración
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public IActionResult ConfiguracionAutotriage()
        {
            TempData["Info"] = "Vista de configuración de autotriage (por implementar)";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Exporta un reporte de asignaciones en formato Excel o PDF
        /// TODO: Implementar exportación de reportes
        /// </summary>
        /// <param name="formato">Formato del reporte (excel, pdf)</param>
        /// <param name="fechaInicio">Fecha inicio del período</param>
        /// <param name="fechaFin">Fecha fin del período</param>
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExportarReporte(string formato, DateTime? fechaInicio, DateTime? fechaFin)
        {
            TempData["Info"] = "Exportación de reportes (por implementar)";
            return RedirectToAction(nameof(Index));
        }
    }
}