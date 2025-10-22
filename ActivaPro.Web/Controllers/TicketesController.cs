using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    public class TicketesController : Controller
    {
        private readonly ITicketesService _service;

        public TicketesController(ITicketesService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int? testUserId, string testUserRole)
        {
            
            int idUsuarioActual = testUserId ?? 1;
            string rolUsuarioActual = testUserRole ?? "Administrador";

            // Guarda en ViewBag para mostrar en la vista
            ViewBag.IdUsuario = idUsuarioActual;
            ViewBag.RolUsuario = rolUsuarioActual;

            // Obtiene los tickets según el rol
            var tickets = await _service.ListByRolAsync(idUsuarioActual, rolUsuarioActual);

            return View(tickets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _service.FindByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }
    }
}