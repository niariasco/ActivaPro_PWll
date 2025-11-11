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
            var categorias = await _context.Categorias
                .Include(c => c.CategoriaEtiquetas).ThenInclude(ce => ce.Etiqueta)
                .Include(c => c.CategoriaEspecialidades).ThenInclude(cs => cs.Especialidad)
                .Include(c => c.CategoriaSLAs).ThenInclude(csla => csla.SLA)
                .ToListAsync();

            return categorias.Select(c => new CategoriasDTO
            {
                id_categoria = c.id_categoria,
                nombre_categoria = c.nombre_categoria,
                Etiquetas = c.CategoriaEtiquetas.Select(x => x.Etiqueta.nombre_etiqueta).ToList(),
                Especialidades = c.CategoriaEspecialidades.Select(x => x.Especialidad.NombreEspecialidad).ToList(),
                id_sla = c.CategoriaSLAs.FirstOrDefault()?.id_sla,
                SLA = c.CategoriaSLAs.FirstOrDefault()?.SLA.descripcion
            });
        }

        public async Task<CategoriasDTO?> FindByIdAsync(int id)
        {
            var c = await _context.Categorias
                .Include(x => x.CategoriaEtiquetas).ThenInclude(ce => ce.Etiqueta)
                .Include(x => x.CategoriaEspecialidades).ThenInclude(cs => cs.Especialidad)
                .Include(x => x.CategoriaSLAs).ThenInclude(csla => csla.SLA)
                .FirstOrDefaultAsync(x => x.id_categoria == id);

            if (c == null) return null;

            var firstSla = c.CategoriaSLAs.FirstOrDefault();
            return new CategoriasDTO
            {
                id_categoria = c.id_categoria,
                nombre_categoria = c.nombre_categoria,
                Etiquetas = c.CategoriaEtiquetas.Select(x => x.Etiqueta.nombre_etiqueta).ToList(),
                Especialidades = c.CategoriaEspecialidades.Select(x => x.Especialidad.NombreEspecialidad).ToList(),
                id_sla = firstSla?.id_sla,
                SLA = firstSla?.SLA.descripcion
            };
        }

        public async Task UpdateAsync(CategoriasDTO dto)
        {
            // Cargar la categoría con las colecciones n-n (joins)
            var entity = await _context.Categorias
                .Include(c => c.CategoriaEtiquetas)
                .Include(c => c.CategoriaEspecialidades)
                .Include(c => c.CategoriaSLAs)
                .FirstOrDefaultAsync(c => c.id_categoria == dto.id_categoria);

            if (entity == null)
                throw new KeyNotFoundException("Categoría no encontrada");

            // Nombre
            entity.nombre_categoria = dto.nombre_categoria;

            // Reemplazar Etiquetas (nombres -> catálogos -> joins)
            entity.CategoriaEtiquetas.Clear();
            if (dto.Etiquetas != null && dto.Etiquetas.Any())
            {
                var nombres = dto.Etiquetas
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .Distinct()
                    .ToList();

                var existentes = await _context.Etiquetas
                    .Where(e => nombres.Contains(e.nombre_etiqueta))
                    .ToListAsync();

                // Crear las que no existan
                var faltantes = nombres.Except(existentes.Select(e => e.nombre_etiqueta)).ToList();
                foreach (var nombre in faltantes)
                {
                    var nueva = new Etiquetas { nombre_etiqueta = nombre };
                    _context.Etiquetas.Add(nueva);
                    existentes.Add(nueva);
                }

                foreach (var et in existentes)
                {
                    entity.CategoriaEtiquetas.Add(new Categoria_Etiqueta
                    {
                        id_categoria = entity.id_categoria,
                        id_etiqueta = et.id_etiqueta,
                        Etiqueta = et
                    });
                }
            }

            // Reemplazar Especialidades (nombres -> catálogos -> joins)
            entity.CategoriaEspecialidades.Clear();
            if (dto.Especialidades != null && dto.Especialidades.Any())
            {
                var nombres = dto.Especialidades
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .Distinct()
                    .ToList();

                var existentes = await _context.Especialidades
                    .Where(e => nombres.Contains(e.NombreEspecialidad))
                    .ToListAsync();

                // Crear las que no existan
                var faltantes = nombres.Except(existentes.Select(e => e.NombreEspecialidad)).ToList();
                foreach (var nombre in faltantes)
                {
                    var nueva = new Especialidades { NombreEspecialidad = nombre };
                    _context.Especialidades.Add(nueva);
                    existentes.Add(nueva);
                }

                foreach (var esp in existentes)
                {
                    entity.CategoriaEspecialidades.Add(new Categoria_Especialidad
                    {
                        id_categoria = entity.id_categoria,
                        id_especialidad = esp.id_especialidad,
                        Especialidad = esp
                    });
                }
            }

            // Reemplazar SLA (n-n, pero DTO actual soporta uno solo)
            entity.CategoriaSLAs.Clear();
            if (dto.id_sla.HasValue && dto.id_sla.Value > 0)
            {
                var sla = await _context.SLA_Tickets
                    .FirstOrDefaultAsync(s => s.id_sla == dto.id_sla.Value);

                if (sla != null)
                {
                    entity.CategoriaSLAs.Add(new Categoria_SLA
                    {
                        id_categoria = entity.id_categoria,
                        id_sla = sla.id_sla,
                        SLA = sla
                    });
                }
            }
            else if (dto.id_sla == -1)
            {
                var slaPersonal = new SLA_Tickets
                {
                    descripcion = string.IsNullOrWhiteSpace(dto.SLA) ? "SLA Personalizado" : dto.SLA!,
                    prioridad = "Media"
                };
                _context.SLA_Tickets.Add(slaPersonal);

                entity.CategoriaSLAs.Add(new Categoria_SLA
                {
                    id_categoria = entity.id_categoria,
                    SLA = slaPersonal
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateAsync(CategoriasDTO dto)
        {
            var entity = new Categorias { nombre_categoria = dto.nombre_categoria };

            // Etiquetas
            var etiquetas = await _context.Etiquetas
                .Where(e => (dto.Etiquetas ?? new List<string>()).Contains(e.nombre_etiqueta))
                .ToListAsync();
            foreach (var et in etiquetas)
                entity.CategoriaEtiquetas.Add(new Categoria_Etiqueta { Etiqueta = et });

            // Especialidades
            var especialidades = await _context.Especialidades
                .Where(e => (dto.Especialidades ?? new List<string>()).Contains(e.NombreEspecialidad))
                .ToListAsync();
            foreach (var es in especialidades)
                entity.CategoriaEspecialidades.Add(new Categoria_Especialidad { Especialidad = es });

            // SLA (permite uno con el DTO actual)
            if (dto.id_sla.HasValue && dto.id_sla.Value > 0)
            {
                var sla = await _context.SLA_Tickets.FirstOrDefaultAsync(s => s.id_sla == dto.id_sla.Value);
                if (sla != null) entity.CategoriaSLAs.Add(new Categoria_SLA { SLA = sla });
            }
            else if (dto.id_sla == -1)
            {
                var slaPersonal = new SLA_Tickets
                {
                    descripcion = string.IsNullOrWhiteSpace(dto.SLA) ? "SLA Personalizado" : dto.SLA!,
                    prioridad = "Media"
                };
                _context.SLA_Tickets.Add(slaPersonal);
                entity.CategoriaSLAs.Add(new Categoria_SLA { SLA = slaPersonal });
            }

            _context.Categorias.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}