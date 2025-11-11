using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    public class TicketesController : Controller
    {
        private readonly ITicketesService _service;
        private readonly IEtiquetasService _etiquetaService;

        public TicketesController(ITicketesService service, IEtiquetasService etiquetaService)
        {
            _service = service;
            _etiquetaService = etiquetaService;
        }

        // GET: Ticketes
        public async Task<IActionResult> Index(int? testUserId, string testUserRole)
        {
            // Variable simulada del usuario actual (en producción vendría de autenticación)
            int idUsuarioActual = testUserId ?? 1;
            string rolUsuarioActual = testUserRole ?? "Administrador";

            // Guardar en ViewBag para mostrar en la vista
            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;

            // Mensajes de éxito o error
            if (TempData["Success"] != null)
                ViewBag.SuccessMessage = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.ErrorMessage = TempData["Error"];

            try
            {
                // Obtener los tickets según el rol
                var tickets = await _service.ListByRolAsync(idUsuarioActual, rolUsuarioActual);
                return View(tickets);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar los tickets: {ex.Message}";
                return View(new List<TicketesDTO>());
            }
        }

        // GET: Ticketes/Details/5
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

        // GET: Ticketes/Create
        public async Task<IActionResult> Create(int? userId)
        {
            // Variable simulada del usuario actual (en producción vendría de autenticación)
            int idUsuarioActual = userId ?? 1;

            try
            {
                // Preparar el DTO con información del usuario
                var dto = await _service.PrepareCreateDTOAsync(idUsuarioActual);

                // Cargar las etiquetas para el selector
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

        // POST: Ticketes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateDTO dto, int? userId)
        {
            // Variable simulada del usuario actual
            int idUsuarioActual = userId ?? 1;
            dto.IdUsuarioSolicitante = idUsuarioActual;

            // Validar el ModelState
            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = "Por favor corrija los errores del formulario.";
                return View(dto);
            }

            try
            {
                // Crear el ticket (incluye cálculos automáticos de SLA y registro en historial)
                int idTicketCreado = await _service.CreateTicketAsync(dto);

                TempData["Success"] = $"✓ Ticket #{idTicketCreado} creado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = idTicketCreado });
            }
            catch (KeyNotFoundException ex)
            {
                // Error: Etiqueta no encontrada
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                // Error: No se encontró categoría o SLA
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                // Error inesperado
                var etiquetas = await _etiquetaService.ListAsync();
                ViewBag.Etiquetas = etiquetas;
                TempData["Error"] = $"Error inesperado al crear el ticket: {ex.Message}";
                return View(dto);
            }
        }
    }
}