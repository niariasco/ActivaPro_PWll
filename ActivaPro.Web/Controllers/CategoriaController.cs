using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Implementations;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActivaPro.Web.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _categoriaService;
        private readonly IEtiquetasService _etiquetaService;
        private readonly IEspecialidadesService _especialidadService;
        private readonly ISlaService _slaService;

        public CategoriaController(
            ICategoriaService categoriaService,
            IEtiquetasService etiquetaService,
            IEspecialidadesService especialidadService,
            ISlaService slaService)
        {
            _categoriaService = categoriaService ?? throw new ArgumentNullException(nameof(categoriaService));
            _etiquetaService = etiquetaService ?? throw new ArgumentNullException(nameof(etiquetaService));
            _especialidadService = especialidadService ?? throw new ArgumentNullException(nameof(especialidadService));
            _slaService = slaService ?? throw new ArgumentNullException(nameof(slaService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaService.ListAsync();
            return View(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _categoriaService.FindByIdAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _categoriaService.FindByIdAsync(id);
            if (categoria == null)
                return NotFound();

            // Obtener las listas completas y convertirlas a strings
            var etiquetas = await _etiquetaService.ListAsync();
            var especialidades = await _especialidadService.ListAsync();
            var slas = await _slaService.ListAsync();

            // Mapear a List<string> para el ViewBag
            ViewBag.Etiquetas = etiquetas.Select(e => e.nombre_etiqueta).ToList();
            ViewBag.Especialidades = especialidades.Select(e => e.NombreEspecialidad).ToList();
            ViewBag.SLAs = slas.Select(s => s.id_sla).ToList();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriasDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                var especialidades = await _especialidadService.ListAsync();
                var slas = await _slaService.ListAsync();

                ViewBag.Etiquetas = etiquetas.Select(e => e.nombre_etiqueta).ToList();
                ViewBag.Especialidades = especialidades.Select(e => e.NombreEspecialidad).ToList();
                ViewBag.SLAs = slas.Select(s => s.id_sla).ToList();

                return View(dto);
            }

            await _categoriaService.UpdateAsync(dto);
            TempData["Success"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var etiquetas = await _etiquetaService.ListAsync();
            var especialidades = await _especialidadService.ListAsync();
            var slas = await _slaService.ListAsync();

            ViewBag.Etiquetas = etiquetas.Select(e => e.nombre_etiqueta).ToList();
            ViewBag.Especialidades = especialidades.Select(e => e.NombreEspecialidad).ToList();
            ViewBag.SLAs = slas.Select(s => s.id_sla).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriasDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                var especialidades = await _especialidadService.ListAsync();
                var slas = await _slaService.ListAsync();

                ViewBag.Etiquetas = etiquetas.Select(e => e.nombre_etiqueta).ToList();
                ViewBag.Especialidades = especialidades.Select(e => e.NombreEspecialidad).ToList();
                ViewBag.SLAs = slas.Select(s => s.id_sla).ToList();

                return View(dto);
            }

            await _categoriaService.CreateAsync(dto);
            TempData["Success"] = "Categoría creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}