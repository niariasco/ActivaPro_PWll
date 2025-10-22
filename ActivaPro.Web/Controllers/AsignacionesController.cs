using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    public class AsignacionesController : Controller
    {
        private readonly IAsignacionesService _service;
        private readonly ITicketesService _ticketService;

        public AsignacionesController(IAsignacionesService service, ITicketesService ticketService)
        {
            _service = service;
            _ticketService = ticketService;
        }

        
        public async Task<IActionResult> Index()
        {
            var tecnicos = await _service.GetAsignacionesPorTecnicoAsync();
            return View(tecnicos);
        }

 
        public async Task<IActionResult> MisAsignaciones(int? id)
        {
            
            int idTecnico = id ?? 2; // Usuario Nicole Arias por defecto

            var tecnico = await _service.GetAsignacionesByTecnicoIdAsync(idTecnico);

            if (tecnico == null)
            {
                return NotFound();
            }

            ViewBag.IdTecnico = idTecnico;
            return View(tecnico);
        }

      
        public async Task<IActionResult> DetalleTicket(int id)
        {
            var ticket = await _ticketService.FindByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View("~/Views/Ticketes/Details.cshtml", ticket);
        }
    }
}