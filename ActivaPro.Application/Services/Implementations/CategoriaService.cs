using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Application.Services.Implementations
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IRepoCategorias _repository;
        private readonly IMapper _mapper;
        private readonly ActivaProContext _context;

        public CategoriaService(IRepoCategorias repository, IMapper mapper, ActivaProContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<CategoriasDTO>> ListAsync()
        {
            var categorias = await _repository.ListAsync();
            return categorias.Select(c => new CategoriasDTO
            {
                id_categoria = c.id_categoria,
                nombre_categoria = c.nombre_categoria,
                Etiquetas = c.CategoriaEtiquetas.Select(e => e.nombre_etiqueta).ToList(),
                Especialidades = c.CategoriaEspecialidades.Select(e => e.NombreEspecialidad).ToList(),
                id_sla = c.SLA_Tickets.FirstOrDefault()?.id_sla,
                SLA = c.SLA_Tickets.FirstOrDefault()?.descripcion
            });
        }

        public async Task<CategoriasDTO?> FindByIdAsync(int id)
        {
            var categoria = await _repository.FindByIdAsync(id);
            if (categoria == null) return null;

            var firstSla = categoria.SLA_Tickets.FirstOrDefault();

            return new CategoriasDTO
            {
                id_categoria = categoria.id_categoria,
                nombre_categoria = categoria.nombre_categoria,
                Etiquetas = categoria.CategoriaEtiquetas.Select(e => e.nombre_etiqueta).ToList(),
                Especialidades = categoria.CategoriaEspecialidades.Select(e => e.NombreEspecialidad).ToList(),
                id_sla = firstSla?.id_sla,
                SLA = firstSla?.descripcion
            };
        }

        public async Task UpdateAsync(CategoriasDTO dto)
        {
            var entity = await _repository.FindByIdAsync(dto.id_categoria);
            if (entity == null) throw new Exception("Categoría no encontrada");

            entity.nombre_categoria = dto.nombre_categoria;

            // Etiquetas
            entity.CategoriaEtiquetas.Clear();
            if (dto.Etiquetas != null && dto.Etiquetas.Any())
            {
                var names = dto.Etiquetas.Distinct().ToList();
                var existentes = await _context.Etiquetas
                    .Where(x => names.Contains(x.nombre_etiqueta))
                    .ToListAsync();

                foreach (var nombre in names)
                {
                    var existente = existentes.FirstOrDefault(x => x.nombre_etiqueta == nombre);
                    if (existente != null)
                    {
                        // Adjuntar existente
                        entity.CategoriaEtiquetas.Add(existente);
                    }
                    else
                    {
                        entity.CategoriaEtiquetas.Add(new Etiquetas { nombre_etiqueta = nombre });
                    }
                }
            }

            // Especialidades
            entity.CategoriaEspecialidades.Clear();
            if (dto.Especialidades != null && dto.Especialidades.Any())
            {
                var names = dto.Especialidades.Distinct().ToList();
                var existentes = await _context.Especialidades
                    .Where(x => names.Contains(x.NombreEspecialidad))
                    .ToListAsync();

                foreach (var nombre in names)
                {
                    var existente = existentes.FirstOrDefault(x => x.NombreEspecialidad == nombre);
                    if (existente != null)
                    {
                        entity.CategoriaEspecialidades.Add(existente);
                    }
                    else
                    {
                        entity.CategoriaEspecialidades.Add(new Especialidades { NombreEspecialidad = nombre });
                    }
                }
            }

            // SLA
            entity.SLA_Tickets.Clear();
            if (dto.id_sla == -1)
            {
                entity.SLA_Tickets.Add(new SLA_Tickets
                {
                    descripcion = string.IsNullOrWhiteSpace(dto.SLA) ? "SLA Personalizado" : dto.SLA,
                    prioridad = "Media",
                    id_categoria = entity.id_categoria
                });
            }
            else if (dto.id_sla.HasValue && dto.id_sla.Value > 0)
            {
                var slaExistente = await _context.SLA_Tickets
                    .FirstOrDefaultAsync(s => s.id_sla == dto.id_sla.Value);

                if (slaExistente != null)
                {
                    // Aseguramos que no se intente insertar duplicado
                    _context.Entry(slaExistente).State = EntityState.Unchanged;
                    // Si la relación implica cambiar el id_categoria del SLA existente, confirmamos el modelo;
                    // Si SLA es independiente, no tocar id_categoria. Si es per categoría, quizá no debiera reutilizarlo.
                    // Aquí asumimos independencia y NO cambiamos id_categoria si ya está asignado a otra categoría.
                    entity.SLA_Tickets.Add(slaExistente);
                }
            }

            await _repository.UpdateAsync(entity);
        }

        public async Task CreateAsync(CategoriasDTO dto)
        {
            var entity = new Categorias
            {
                nombre_categoria = dto.nombre_categoria
            };

            // Reutilizar Etiquetas existentes
            entity.CategoriaEtiquetas = new List<Etiquetas>();
            if (dto.Etiquetas?.Any() == true)
            {
                var etiquetasExist = await _context.Etiquetas
                    .Where(x => dto.Etiquetas.Contains(x.nombre_etiqueta))
                    .ToListAsync();

                foreach (var nombre in dto.Etiquetas.Distinct())
                {
                    var existente = etiquetasExist.FirstOrDefault(x => x.nombre_etiqueta == nombre);
                    entity.CategoriaEtiquetas.Add(existente ?? new Etiquetas { nombre_etiqueta = nombre });
                }
            }

            // Reutilizar Especialidades existentes
            entity.CategoriaEspecialidades = new List<Especialidades>();
            if (dto.Especialidades?.Any() == true)
            {
                var especialidadesExist = await _context.Especialidades
                    .Where(x => dto.Especialidades.Contains(x.NombreEspecialidad))
                    .ToListAsync();

                foreach (var nombre in dto.Especialidades.Distinct())
                {
                    var existente = especialidadesExist.FirstOrDefault(x => x.NombreEspecialidad == nombre);
                    entity.CategoriaEspecialidades.Add(existente ?? new Especialidades { NombreEspecialidad = nombre });
                }
            }

            // SLA
            entity.SLA_Tickets = new List<SLA_Tickets>();
            if (dto.id_sla == -1)
            {
                entity.SLA_Tickets.Add(new SLA_Tickets
                {
                    descripcion = string.IsNullOrWhiteSpace(dto.SLA) ? "SLA Personalizado" : dto.SLA!,
                    prioridad = "Media",

                });
            }
            else if (dto.id_sla.HasValue && dto.id_sla.Value > 0)
            {
                var slaExistente = await _context.SLA_Tickets
                    .FirstOrDefaultAsync(s => s.id_sla == dto.id_sla.Value);

                if (slaExistente != null)
                {
                    _context.Entry(slaExistente).State = EntityState.Unchanged;
                    entity.SLA_Tickets.Add(slaExistente);
                }
            }

            await _repository.CreateAsync(entity);
        }
    }
}