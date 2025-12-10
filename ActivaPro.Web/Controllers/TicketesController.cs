using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    [Authorize]
    public class TicketesController : Controller
    {
        private readonly ITicketesService _service;
        private readonly IEtiquetasService _etiquetaService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TicketesController(
            ITicketesService service,
            IEtiquetasService etiquetaService,
            IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _etiquetaService = etiquetaService;
            _webHostEnvironment = webHostEnvironment;
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

        // ========== INDEX ==========
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();
            string nombreUsuarioActual = GetCurrentUserName();

            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;
            ViewBag.NombreUsuario = nombreUsuarioActual;

            if (TempData["Success"] != null)
                ViewBag.SuccessMessage = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.ErrorMessage = TempData["Error"];

            try
            {
                var tickets = await _service.ListByRolAsync(idUsuarioActual, rolUsuarioActual);
                return View(tickets);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar los tickets: {ex.Message}";
                return View(new List<TicketesDTO>());
            }
        }

        // ========== DETAILS ==========
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                int idUsuarioActual = GetCurrentUserId();
                string rolUsuarioActual = GetCurrentUserRole();

                var ticket = await _service.FindByIdAsync(id);

                if (ticket == null)
                {
                    TempData["Error"] = $"El ticket con ID {id} no existe o fue eliminado.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar acceso según rol
                bool tieneAcceso = false;
                switch (rolUsuarioActual.ToLower())
                {
                    case "administrador":
                    case "coordinador":
                        tieneAcceso = true;
                        break;
                    case "técnico":
                    case "tecnico":
                        tieneAcceso = ticket.IdUsuarioAsignado == idUsuarioActual ||
                                     ticket.IdUsuarioAsignado == null;
                        break;
                    case "cliente":
                        tieneAcceso = ticket.IdUsuarioSolicitante == idUsuarioActual;
                        break;
                }

                if (!tieneAcceso)
                {
                    TempData["Error"] = "No tienes permiso para ver este ticket.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = GetCurrentUserName();

                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el detalle del ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== CREATE - GET ==========
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            if (rolUsuarioActual.ToLower() != "cliente")
            {
                TempData["Error"] = "Solo los clientes pueden crear tickets.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var dto = await _service.PrepareCreateDTOAsync(idUsuarioActual);
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al preparar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== CREATE - POST ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateDTO dto)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            if (rolUsuarioActual.ToLower() != "cliente")
            {
                TempData["Error"] = "Solo los clientes pueden crear tickets.";
                return RedirectToAction(nameof(Index));
            }

            dto.IdUsuarioSolicitante = idUsuarioActual;

            if (dto.ImagenesAdjuntas != null && dto.ImagenesAdjuntas.Any())
            {
                if (dto.ImagenesAdjuntas.Count > 5)
                {
                    ModelState.AddModelError("ImagenesAdjuntas", "No puede adjuntar más de 5 imágenes");
                }

                foreach (var imagen in dto.ImagenesAdjuntas.Where(i => i != null))
                {
                    if (imagen.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImagenesAdjuntas",
                            $"La imagen '{imagen.FileName}' excede el tamaño máximo de 5MB");
                    }

                    var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("ImagenesAdjuntas",
                            $"La imagen '{imagen.FileName}' no tiene un formato válido.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                return View(dto);
            }

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                int idTicketCreado = await _service.CreateTicketAsync(dto, uploadsFolder);

                TempData["Success"] = $"Ticket #{idTicketCreado} creado exitosamente";
                return RedirectToAction(nameof(Details), new { id = idTicketCreado });
            }
            catch (Exception ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = $"Error al crear el ticket: {ex.Message}";
                return View(dto);
            }
        }

        // ========== EDIT - GET ==========
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO TÉCNICOS pueden editar tickets
            if (rolUsuarioActual.ToLower() == "cliente")
            {
                TempData["Error"] = "Los clientes NO pueden editar tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() == "administrador")
            {
                TempData["Error"] = "Los administradores NO pueden editar tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() != "técnico" && rolUsuarioActual.ToLower() != "tecnico")
            {
                TempData["Error"] = "Solo los técnicos pueden editar tickets.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ticket = await _service.FindByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = $"El ticket con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar que el técnico tenga el ticket asignado
                if (ticket.IdUsuarioAsignado != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puedes editar tickets que están asignados a ti.";
                    return RedirectToAction(nameof(Index));
                }

                // ⭐ PASAR EL ROL AL SERVICIO
                var dto = await _service.PrepareEditDTOAsync(id, rolUsuarioActual);

                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== EDIT - POST ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditDTO dto)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO TÉCNICOS
            if (rolUsuarioActual.ToLower() != "técnico" && rolUsuarioActual.ToLower() != "tecnico")
            {
                TempData["Error"] = "Solo los técnicos pueden editar tickets.";
                return RedirectToAction(nameof(Index));
            }

            var ticketExistente = await _service.FindByIdAsync(dto.IdTicket);
            if (ticketExistente == null)
            {
                TempData["Error"] = "El ticket no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (ticketExistente.IdUsuarioAsignado != idUsuarioActual)
            {
                TempData["Error"] = "Solo puedes editar tickets que están asignados a ti.";
                return RedirectToAction(nameof(Index));
            }

            // Validar estado según flujo secuencial
            var estadosPermitidos = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuarioActual, ticketExistente.Estado);
            if (!estadosPermitidos.Contains(dto.Estado))
            {
                var siguienteEstado = TicketEditDTO.ObtenerSiguienteEstadoPermitido(ticketExistente.Estado);
                if (siguienteEstado != null)
                {
                    ModelState.AddModelError("Estado",
                        $"Flujo inválido. Desde '{ticketExistente.Estado}' solo puedes cambiar a '{siguienteEstado}'.");
                }
                else
                {
                    ModelState.AddModelError("Estado",
                        $"El ticket en estado '{ticketExistente.Estado}' no puede cambiar más. Solo un administrador o cliente puede cerrarlo.");
                }
            }

            // Validar imágenes
            if (dto.NuevasImagenes != null && dto.NuevasImagenes.Any())
            {
                int totalImagenes = (dto.ImagenesExistentes?.Count ?? 0) + dto.NuevasImagenes.Count;
                if (dto.ImagenesAEliminar != null)
                    totalImagenes -= dto.ImagenesAEliminar.Count;

                if (totalImagenes > 5)
                {
                    ModelState.AddModelError("NuevasImagenes",
                        "El total de imágenes no puede exceder 5.");
                }

                foreach (var imagen in dto.NuevasImagenes.Where(i => i != null))
                {
                    if (imagen.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("NuevasImagenes",
                            $"La imagen '{imagen.FileName}' excede el tamaño máximo de 5MB");
                    }

                    var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("NuevasImagenes",
                            $"La imagen '{imagen.FileName}' no tiene un formato válido.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                dto.EstadosDisponibles = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuarioActual, ticketExistente.Estado);

                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                TempData["Error"] = "Por favor corrija los errores del formulario.";
                return View(dto);
            }

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // ⭐ PASAR EL ROL AL SERVICIO
                await _service.UpdateTicketAsync(dto, uploadsFolder, idUsuarioActual, rolUsuarioActual);

                TempData["Success"] = $"✅ Ticket #{dto.IdTicket} actualizado exitosamente a estado: {dto.Estado}";
                return RedirectToAction(nameof(Details), new { id = dto.IdTicket });
            }
            catch (InvalidOperationException ex)
            {
                dto.EstadosDisponibles = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuarioActual, ticketExistente.Estado);

                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                TempData["Error"] = $"❌ Error de validación: {ex.Message}";
                return View(dto);
            }
            catch (Exception ex)
            {
                dto.EstadosDisponibles = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuarioActual, ticketExistente.Estado);

                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                TempData["Error"] = $"Error al actualizar el ticket: {ex.Message}";
                return View(dto);
            }
        }

        // ========== ⭐ CAMBIO RÁPIDO DE ESTADO CON COMENTARIO ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoRapido(int idTicket, string nuevoEstado, string comentario)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: Solo técnicos
            if (rolUsuarioActual.ToLower() != "técnico" && rolUsuarioActual.ToLower() != "tecnico")
            {
                return Json(new
                {
                    success = false,
                    message = "⛔ Solo los técnicos pueden cambiar el estado de los tickets."
                });
            }

            // VALIDACIÓN: Comentario obligatorio
            if (string.IsNullOrWhiteSpace(comentario) || comentario.Length < 10)
            {
                return Json(new
                {
                    success = false,
                    message = "⚠️ El comentario es obligatorio y debe tener al menos 10 caracteres."
                });
            }

            if (comentario.Length > 500)
            {
                return Json(new
                {
                    success = false,
                    message = "⚠️ El comentario no puede exceder 500 caracteres."
                });
            }

            try
            {
                var ticket = await _service.FindByIdAsync(idTicket);
                if (ticket == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ El ticket no existe."
                    });
                }

                // Llamar al servicio CON el comentario
                await _service.CambiarEstadoRapidoAsync(idTicket, nuevoEstado, idUsuarioActual, comentario);

                // Mensajes personalizados según el nuevo estado
                string emoji = nuevoEstado switch
                {
                    "Asignado" => "👤",
                    "En Proceso" => "⚙️",
                    "Resuelto" => "✅",
                    _ => "📝"
                };

                string mensaje = nuevoEstado switch
                {
                    "Asignado" => "Ticket asignado correctamente. Ahora puedes comenzar a trabajar en él.",
                    "En Proceso" => "Estado cambiado a 'En Proceso'. ¡Estás trabajando en este ticket!",
                    "Resuelto" => "Estado cambiado a 'Resuelto'. El cliente puede revisar y cerrar el ticket.",
                    _ => $"Estado cambiado a '{nuevoEstado}' exitosamente."
                };

                return Json(new
                {
                    success = true,
                    message = $"{emoji} {mensaje}",
                    nuevoEstado = nuevoEstado,
                    comentario = comentario
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                // Logging detallado
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en CambiarEstadoRapido:");
                System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = $"❌ Error inesperado: {ex.Message}",
                    detalle = ex.InnerException?.Message
                });
            }
        }

        // ========== CLOSE - GET ==========
        [HttpGet]
        public async Task<IActionResult> Close(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden cerrar tickets.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ticket = await _service.FindByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = $"El ticket con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (ticket.Estado.ToLower() == "cerrado")
                {
                    TempData["Error"] = $"El ticket #{id} ya está cerrado.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (rolUsuarioActual.ToLower() == "cliente" && ticket.IdUsuarioSolicitante != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puede cerrar sus propios tickets.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.IdUsuario = idUsuarioActual;
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== CLOSE - POST ==========
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden cerrar tickets.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ticket = await _service.FindByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = $"El ticket con ID {id} no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (ticket.Estado.ToLower() == "cerrado")
                {
                    TempData["Error"] = $"El ticket #{id} ya está cerrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (rolUsuarioActual.ToLower() == "cliente" && ticket.IdUsuarioSolicitante != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puede cerrar sus propios tickets.";
                    return RedirectToAction(nameof(Index));
                }

                await _service.CloseTicketAsync(id, idUsuarioActual);

                TempData["Success"] = $"Ticket #{id} cerrado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cerrar el ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}