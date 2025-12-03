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
    [Authorize] // Requiere autenticación para acceder a cualquier acción
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

        // ========== MÉTODOS AUXILIARES PARA OBTENER USUARIO ACTUAL ==========

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
            return User.FindFirstValue("rol") ?? User.FindFirstValue(ClaimTypes.Role) ?? "Cliente";
        }

        /// <summary>
        /// Obtiene el nombre del usuario autenticado desde los Claims
        /// </summary>
        private string GetCurrentUserName()
        {
            return User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
        }

        // ========== ACCIONES DEL CONTROLADOR ==========

        /// <summary>
        /// GET: Ticketes/Index - Lista todos los tickets según el rol del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtener información del usuario autenticado
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();
            string nombreUsuarioActual = GetCurrentUserName();

            // Pasar información a la vista
            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;
            ViewBag.NombreUsuario = nombreUsuarioActual;

            // Mensajes de TempData
            if (TempData["Success"] != null)
                ViewBag.SuccessMessage = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.ErrorMessage = TempData["Error"];

            try
            {
                // Listar tickets según el rol del usuario
                var tickets = await _service.ListByRolAsync(idUsuarioActual, rolUsuarioActual);
                return View(tickets);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar los tickets: {ex.Message}";
                return View(new List<TicketesDTO>());
            }
        }

        /// <summary>
        /// GET: Ticketes/Details/5 - Muestra los detalles de un ticket específico
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Obtener información del usuario autenticado
                int idUsuarioActual = GetCurrentUserId();
                string rolUsuarioActual = GetCurrentUserRole();
                string nombreUsuarioActual = GetCurrentUserName();

                // Obtener el ticket
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
                        // Admin y Coordinador pueden ver TODOS los tickets
                        tieneAcceso = true;
                        break;

                    case "técnico":
                    case "tecnico":
                        // Técnico puede ver:
                        // 1. Tickets asignados a él
                        // 2. Tickets sin asignar (para poder trabajarlos)
                        tieneAcceso = ticket.IdUsuarioAsignado == idUsuarioActual ||
                                     ticket.IdUsuarioAsignado == null ||
                                     !ticket.IdUsuarioAsignado.HasValue;
                        break;

                    case "cliente":
                        // Cliente solo puede ver sus propios tickets
                        tieneAcceso = ticket.IdUsuarioSolicitante == idUsuarioActual;
                        break;

                    default:
                        tieneAcceso = false;
                        break;
                }

                if (!tieneAcceso)
                {
                    TempData["Error"] = "No tienes permiso para ver este ticket.";
                    return RedirectToAction(nameof(Index));
                }

                // Pasar datos a la vista
                ViewBag.IdUsuario = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                ViewBag.NombreUsuario = nombreUsuarioActual;

                // Mensajes de TempData
                if (TempData["Success"] != null)
                    ViewBag.SuccessMessage = TempData["Success"];
                if (TempData["Error"] != null)
                    ViewBag.ErrorMessage = TempData["Error"];

                return View(ticket);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Debe iniciar sesión para ver los detalles del ticket.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el detalle del ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Ticketes/Create - Muestra el formulario para crear un nuevo ticket
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO CLIENTES pueden crear tickets
            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden crear tickets. Solo pueden editar y visualizar tickets asignados.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() == "administrador")
            {
                TempData["Error"] = "Los administradores NO pueden crear tickets. Solo los clientes pueden crear tickets de soporte.";
                return RedirectToAction(nameof(Index));
            }

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
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al preparar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: Ticketes/Create - Procesa la creación de un nuevo ticket
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateDTO dto)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO CLIENTES pueden crear tickets
            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden crear tickets. Solo pueden editar y visualizar tickets asignados.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() == "administrador")
            {
                TempData["Error"] = "Los administradores NO pueden crear tickets. Solo los clientes pueden crear tickets de soporte.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() != "cliente")
            {
                TempData["Error"] = "Solo los clientes pueden crear tickets.";
                return RedirectToAction(nameof(Index));
            }

            dto.IdUsuarioSolicitante = idUsuarioActual;

            // Validar imágenes adjuntas
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
                            $"La imagen '{imagen.FileName}' no tiene un formato válido. Solo se permiten: JPG, PNG, GIF, BMP");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
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

                int idTicketCreado = await _service.CreateTicketAsync(dto, uploadsFolder);

                string mensajeExito = $"Ticket #{idTicketCreado} creado exitosamente";
                if (dto.ImagenesAdjuntas != null && dto.ImagenesAdjuntas.Any())
                {
                    mensajeExito += $" con {dto.ImagenesAdjuntas.Count} imagen(es) adjuntada(s)";
                }

                TempData["Success"] = mensajeExito;
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

        /// <summary>
        /// GET: Ticketes/Edit/5 - Muestra el formulario para editar un ticket
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO TÉCNICOS pueden editar tickets
            if (rolUsuarioActual.ToLower() == "cliente")
            {
                TempData["Error"] = "Los clientes NO pueden editar tickets. Solo pueden visualizar y cerrar sus propios tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() == "administrador")
            {
                TempData["Error"] = "Los administradores NO pueden editar tickets. Solo pueden visualizar y cerrar tickets.";
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

                var dto = await _service.PrepareEditDTOAsync(id);
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

        /// <summary>
        /// POST: Ticketes/Edit/5 - Procesa la edición de un ticket
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditDTO dto)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: SOLO TÉCNICOS pueden editar tickets
            if (rolUsuarioActual.ToLower() == "cliente")
            {
                TempData["Error"] = "Los clientes NO pueden editar tickets. Solo pueden visualizar y cerrar sus propios tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() == "administrador")
            {
                TempData["Error"] = "Los administradores NO pueden editar tickets. Solo pueden visualizar y cerrar tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() != "técnico" && rolUsuarioActual.ToLower() != "tecnico")
            {
                TempData["Error"] = "Solo los técnicos pueden editar tickets.";
                return RedirectToAction(nameof(Index));
            }

            // Validar permisos antes de procesar
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

            // Validar imágenes
            if (dto.NuevasImagenes != null && dto.NuevasImagenes.Any())
            {
                int totalImagenes = (dto.ImagenesExistentes?.Count ?? 0) + dto.NuevasImagenes.Count;
                if (dto.ImagenesAEliminar != null)
                    totalImagenes -= dto.ImagenesAEliminar.Count;

                if (totalImagenes > 5)
                {
                    ModelState.AddModelError("NuevasImagenes",
                        "El total de imágenes no puede exceder 5. Elimine algunas imágenes existentes primero.");
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

                await _service.UpdateTicketAsync(dto, uploadsFolder, idUsuarioActual);

                TempData["Success"] = $"Ticket #{dto.IdTicket} actualizado exitosamente";
                return RedirectToAction(nameof(Details), new { id = dto.IdTicket });
            }
            catch (Exception ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                ViewBag.RolUsuario = rolUsuarioActual;
                TempData["Error"] = $"Error al actualizar el ticket: {ex.Message}";
                return View(dto);
            }
        }

        /// <summary>
        /// GET: Ticketes/Close/5 - Muestra el formulario de confirmación para cerrar un ticket
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Close(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: Solo Cliente o Administrador pueden cerrar tickets
            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden cerrar tickets. Solo pueden editar y visualizar tickets asignados.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() != "cliente" && rolUsuarioActual.ToLower() != "administrador")
            {
                TempData["Error"] = "Solo los clientes y administradores pueden cerrar tickets.";
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

                // Si es cliente, validar que sea el solicitante
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

        /// <summary>
        /// POST: Ticketes/Close/5 - Procesa el cierre de un ticket
        /// </summary>
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id)
        {
            int idUsuarioActual = GetCurrentUserId();
            string rolUsuarioActual = GetCurrentUserRole();

            // VALIDACIÓN: Solo Cliente o Administrador pueden cerrar
            if (rolUsuarioActual.ToLower() == "técnico" || rolUsuarioActual.ToLower() == "tecnico")
            {
                TempData["Error"] = "Los técnicos NO pueden cerrar tickets. Solo pueden editar y visualizar tickets asignados.";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuarioActual.ToLower() != "cliente" && rolUsuarioActual.ToLower() != "administrador")
            {
                TempData["Error"] = "Solo los clientes y administradores pueden cerrar tickets.";
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

                // Validar que no esté ya cerrado
                if (ticket.Estado.ToLower() == "cerrado")
                {
                    TempData["Error"] = $"El ticket #{id} ya está cerrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Si es cliente, validar que sea el propietario
                if (rolUsuarioActual.ToLower() == "cliente" && ticket.IdUsuarioSolicitante != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puede cerrar sus propios tickets.";
                    return RedirectToAction(nameof(Index));
                }

                // Cerrar el ticket
                await _service.CloseTicketAsync(id, idUsuarioActual);

                TempData["Success"] = $"Ticket #{id} cerrado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"{ex.Message}";
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