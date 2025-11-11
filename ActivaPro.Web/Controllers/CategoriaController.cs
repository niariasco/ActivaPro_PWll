using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            _categoriaService = categoriaService;
            _etiquetaService = etiquetaService;
            _especialidadService = especialidadService;
            _slaService = slaService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaService.ListAsync();
            if (TempData["Success"] != null) ViewBag.SuccessMessage = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.ErrorMessage = TempData["Error"];
            return View(categorias);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var categoria = await _categoriaService.FindByIdAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = $"Categoría con ID {id} no existe o fue eliminada.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _categoriaService.FindByIdAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "La categoría solicitada no existe.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectLists(categoria);
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriasDTO dto)
        {
            // Precargar descripción y prioridad del SLA seleccionado (si se escogió uno existente)
            await HydrateSlaSelection(dto);

            if (!ModelState.IsValid)
            {
                await PopulateSelectLists(dto);
                TempData["Error"] = "Errores de validación: " + string.Join("; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(dto);
            }

            try
            {
                await _categoriaService.UpdateAsync(dto);
                TempData["Success"] = "✓ Categoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await PopulateSelectLists(dto);
                TempData["Error"] = $"Error al actualizar la categoría: {ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriasDTO dto)
            {
                await HydrateSlaSelection(dto);

                if (!ModelState.IsValid)
                {
                    await PopulateSelectLists(dto);
                    TempData["Error"] = "Errores de validación: " + string.Join("; ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(dto);
                }

                try
                {
                    await _categoriaService.CreateAsync(dto);
                    TempData["Success"] = "✓ Categoría creada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await PopulateSelectLists(dto);
                    TempData["Error"] = $"Error al crear la categoría: {ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}";
                    return View(dto);
                }
            }

            private async Task HydrateSlaSelection(CategoriasDTO dto)
            {
                // Si seleccionó SLA existente (id_sla > 0) obtener su descripción (y prioridad si hicieras uso)
                if (dto.id_sla.HasValue && dto.id_sla.Value > 0)
                {
                    var slas = await _slaService.ListAsync();
                    var sla = slas.FirstOrDefault(s => s.id_sla == dto.id_sla.Value);
                    if (sla != null)
                    {
                        dto.SLA = sla.descripcion ?? string.Empty;
                    }
                }
                else if (dto.id_sla == -1)
                {
                    // SLA personalizado: asegúrate de que dto.SLA tenga algo o asigna etiqueta
                    if (string.IsNullOrWhiteSpace(dto.SLA))
                    {
                        dto.SLA = "SLA Personalizado";
                    }
                }
            }

            private async Task PopulateSelectLists(CategoriasDTO? dto = null)
            {
                var etiquetas = await _etiquetaService.ListAsync();
                var especialidades = await _especialidadService.ListAsync();
                var slas = await _slaService.ListAsync();

                ViewBag.Etiquetas = etiquetas.Select(e => e.nombre_etiqueta).ToList();
                ViewBag.Especialidades = especialidades.Select(e => e.NombreEspecialidad).ToList();
                ViewBag.SLAs = slas.Select(s => s.id_sla).ToList();
                ViewBag.SLAsDescripciones = slas.ToDictionary(s => s.id_sla, s => s.descripcion ?? $"SLA {s.id_sla}");
            }
        }
    }