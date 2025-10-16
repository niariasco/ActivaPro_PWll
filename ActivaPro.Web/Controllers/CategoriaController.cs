using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ActivaPro.Web.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categorias = await _service.ListAsync();
            return View(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _service.FindByIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }
    }
}