using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ActivaPro.Web.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly ITecnicoService _service;

        public TecnicoController(ITecnicoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tecnicos = await _service.ListAsync();
            return View(tecnicos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var tecnico = await _service.FindByIdAsync(id);
            if (tecnico == null)
                return NotFound();

            return View(tecnico);
        }
    }
}
