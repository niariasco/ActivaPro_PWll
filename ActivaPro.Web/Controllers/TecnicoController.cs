using ActivaPro.Application.DTOs;
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
            if (TempData["Success"] != null) ViewBag.SuccessMessage = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.ErrorMessage = TempData["Error"];
            return View(tecnicos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var tecnico = await _service.FindByIdAsync(id);
            if (tecnico == null)
            {
                TempData["Error"] = "El técnico no existe.";
                return RedirectToAction(nameof(Index));
            }
            return View(tecnico);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadEspecialidadesU(null);
            return View(new TecnicosDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TecnicosDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadEspecialidadesU(dto.EspecialidadesIds);
                TempData["Error"] = "Corrija los errores del formulario.";
                return View(dto);
            }

            await _service.CreateAsync(dto);
            TempData["Success"] = "✓ Técnico creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tecnico = await _service.FindByIdAsync(id);
            if (tecnico == null)
            {
                TempData["Error"] = "El técnico no existe.";
                return RedirectToAction(nameof(Index));
            }
            await LoadEspecialidadesU(tecnico.EspecialidadesIds);
            return View(tecnico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TecnicosDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadEspecialidadesU(dto.EspecialidadesIds);
                TempData["Error"] = "Corrija los errores del formulario.";
                return View(dto);
            }

            await _service.UpdateAsync(dto);
            TempData["Success"] = "✓ Técnico actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadEspecialidadesU(IEnumerable<int>? seleccion)
        {
            var catalogo = await _service.GetEspecialidadesUCatalogAsync();
            // Proyectar a shape estable para la vista
            ViewBag.EspecialidadesCatalogo = catalogo.Select(c => new { Id = c.Id, Nombre = c.Nombre }).ToList();
            ViewBag.SelectedIds = seleccion?.ToList() ?? new List<int>();
        }
    }
}