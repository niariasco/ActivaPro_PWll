using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IRepoCategorias _repository;
        private readonly IMapper _mapper;

        public CategoriaService(IRepoCategorias repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoriasDTO>> ListAsync()
        {
            var categorias = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<CategoriasDTO>>(categorias);
        }

        public async Task<CategoriasDTO?> FindByIdAsync(int id)
        {
            var categoria = await _repository.FindByIdAsync(id);
            if (categoria == null) return null;

            return new CategoriasDTO
            {
                id_categoria = categoria.id_categoria,
                nombre_categoria = categoria.nombre_categoria,
                // Etiquetas asignadas
                Etiquetas = categoria.CategoriaEtiquetas
                    .Select(e => e.nombre_etiqueta)
                    .ToList(),
                // Especialidades asignadas
                Especialidades = categoria.CategoriaEspecialidades
                    .Select(e => e.NombreEspecialidad)
                    .ToList(),

                SLA = categoria.SLA_Tickets
                    .Select(s => s.descripcion)
                    .FirstOrDefault() ?? string.Empty 
            };
        }

        public async Task UpdateAsync(CategoriasDTO dto)
        {
            var entity = await _repository.FindByIdAsync(dto.id_categoria);
            if (entity == null) throw new Exception("Categoría no encontrada");

            // Actualizar nombre
            entity.nombre_categoria = dto.nombre_categoria;

            // Limpiar relaciones existentes
            entity.CategoriaEtiquetas.Clear();
            entity.CategoriaEspecialidades.Clear();
            entity.SLA_Tickets.Clear();

            // Agregar nuevas etiquetas
            foreach (var etiqueta in dto.Etiquetas)
            {
                entity.CategoriaEtiquetas.Add(new Etiquetas { nombre_etiqueta = etiqueta });
            }

            // Agregar nuevas especialidades
            foreach (var especialidad in dto.Especialidades)
            {
                entity.CategoriaEspecialidades.Add(new Especialidades { NombreEspecialidad = especialidad });
            }

            if (!string.IsNullOrEmpty(dto.SLA))
            {
                entity.SLA_Tickets.Add(new SLA_Tickets
                {
                    descripcion = dto.SLA,
                    prioridad = "Media"
                });
            }

            await _repository.UpdateAsync(entity);
        }

        public async Task CreateAsync(CategoriasDTO dto)
        {
            var entity = new Categorias
            {
                nombre_categoria = dto.nombre_categoria
            };

            entity.CategoriaEtiquetas = dto.Etiquetas.Select(e => new Etiquetas
            {
                nombre_etiqueta = e
            }).ToList();

            entity.CategoriaEspecialidades = dto.Especialidades.Select(e => new Especialidades
            {
                NombreEspecialidad = e
            }).ToList();

            entity.SLA_Tickets = new List<SLA_Tickets>();
            if (!string.IsNullOrEmpty(dto.SLA))
            {
                entity.SLA_Tickets.Add(new SLA_Tickets
                {
                    descripcion = dto.SLA,
                    prioridad = "Media"
                });
            }

            await _repository.CreateAsync(entity);
        }
    }
}