using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
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


        public async Task<IActionResult> Index(int? testUserId, string testUserRole)
        {
            int idUsuarioActual = testUserId ?? 1;
            string rolUsuarioActual = testUserRole ?? "Administrador";

            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;

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
                return View(new System.Collections.Generic.List<TicketesDTO>());
            }
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ticket = await _service.FindByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = $"El ticket con ID {id} no existe o fue eliminado.";
                    return RedirectToAction(nameof(Index));
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el detalle del ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> Create(int? userId)
        {
            int idUsuarioActual = userId ?? 1;

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateDTO dto, int? userId)
        {
            int idUsuarioActual = userId ?? 1;
            dto.IdUsuarioSolicitante = idUsuarioActual;

            if (dto.ImagenesAdjuntas != null && dto.ImagenesAdjuntas.Any())
            {
                // Validar cantidad (máximo 5)
                if (dto.ImagenesAdjuntas.Count > 5)
                {
                    ModelState.AddModelError("ImagenesAdjuntas", "No puede adjuntar más de 5 imágenes");
                }

                // Validar cada imagen
                foreach (var imagen in dto.ImagenesAdjuntas.Where(i => i != null))
                {
                    // Validar tamaño (máximo 5MB)
                    if (imagen.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImagenesAdjuntas",
                            $"La imagen '{imagen.FileName}' excede el tamaño máximo de 5MB");
                    }

                    // Validar extensión
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
                // Crear carpeta para imágenes si no existe
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Crear el ticket con imágenes
                int idTicketCreado = await _service.CreateTicketAsync(dto, uploadsFolder);

                string mensajeExito = $"✓ Ticket #{idTicketCreado} creado exitosamente";
                if (dto.ImagenesAdjuntas != null && dto.ImagenesAdjuntas.Any())
                {
                    mensajeExito += $" con {dto.ImagenesAdjuntas.Count} imagen(es) adjuntada(s)";
                }

                TempData["Success"] = mensajeExito;
                return RedirectToAction(nameof(Details), new { id = idTicketCreado });
            }
            catch (KeyNotFoundException ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = $"Error inesperado al crear el ticket: {ex.Message}";
                return View(dto);
            }
        }


        public async Task<IActionResult> Edit(int id, int? userId)
        {
            int idUsuarioActual = userId ?? 1;

            try
            {
                var dto = await _service.PrepareEditDTOAsync(id);
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;

                return View(dto);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditDTO dto, int? userId)
        {
            int idUsuarioActual = userId ?? 1;


            if (dto.NuevasImagenes != null && dto.NuevasImagenes.Any())
            {
                // Validar cantidad total (existentes + nuevas <= 5)
                int totalImagenes = (dto.ImagenesExistentes?.Count ?? 0) + dto.NuevasImagenes.Count;
                if (dto.ImagenesAEliminar != null)
                    totalImagenes -= dto.ImagenesAEliminar.Count;

                if (totalImagenes > 5)
                {
                    ModelState.AddModelError("NuevasImagenes",
                        "El total de imágenes no puede exceder 5. Elimine algunas imágenes existentes primero.");
                }

                // Validar cada nueva imagen
                foreach (var imagen in dto.NuevasImagenes.Where(i => i != null))
                {
                    // Validar tamaño (máximo 5MB)
                    if (imagen.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("NuevasImagenes",
                            $"La imagen '{imagen.FileName}' excede el tamaño máximo de 5MB");
                    }

                    // Validar extensión
                    var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("NuevasImagenes",
                            $"La imagen '{imagen.FileName}' no tiene un formato válido. Solo se permiten: JPG, PNG, GIF, BMP");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                TempData["Error"] = "Por favor corrija los errores del formulario.";
                return View(dto);
            }

            try
            {
                // Crear carpeta para imágenes si no existe
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Actualizar el ticket
                await _service.UpdateTicketAsync(dto, uploadsFolder, idUsuarioActual);

                TempData["Success"] = $"✓ Ticket #{dto.IdTicket} actualizado exitosamente";
                return RedirectToAction(nameof(Details), new { id = dto.IdTicket });
            }
            catch (KeyNotFoundException ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                ViewBag.IdUsuarioActual = idUsuarioActual;
                TempData["Error"] = $"Error al actualizar el ticket: {ex.Message}";
                return View(dto);
            }
        }

        

        public async Task<IActionResult> Close(int id, int? userId, string userRole)
        {
            int idUsuarioActual = userId ?? 1;
            string rolUsuarioActual = userRole ?? "Administrador";

            // Validar que solo Cliente o Administrador puedan cerrar tickets
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

                // Validar que el ticket no esté ya cerrado
                if (ticket.Estado.ToLower() == "cerrado")
                {
                    TempData["Error"] = $"El ticket #{id} ya está cerrado.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Si es cliente, validar que sea el solicitante del ticket
                if (rolUsuarioActual.ToLower() == "cliente" && ticket.IdUsuarioSolicitante != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puede cerrar sus propios tickets.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.RolUsuario = rolUsuarioActual;
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id, int? userId, string userRole)
        {
            int idUsuarioActual = userId ?? 1;
            string rolUsuarioActual = userRole ?? "Administrador";

            // Validar permisos nuevamente
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

                // Validar que no esté cerrado
                if (ticket.Estado.ToLower() == "cerrado")
                {
                    TempData["Error"] = $"El ticket #{id} ya está cerrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Si es cliente, validar pertenencia
                if (rolUsuarioActual.ToLower() == "cliente" && ticket.IdUsuarioSolicitante != idUsuarioActual)
                {
                    TempData["Error"] = "Solo puede cerrar sus propios tickets.";
                    return RedirectToAction(nameof(Index));
                }

                // Cerrar el ticket
                await _service.CloseTicketAsync(id, idUsuarioActual);

                TempData["Success"] = $"✓ Ticket #{id} cerrado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
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