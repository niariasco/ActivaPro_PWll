using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class CategoriasRepo : IRepoCategorias
    {
        private readonly ActivaProContext _context;

        public CategoriasRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<Categorias?> FindByIdAsync(int id)
        {
            return await _context.Categorias
                .Include(c => c.CategoriaEtiquetas)
                    .ThenInclude(ce => ce.Etiqueta)
                .Include(c => c.CategoriaEspecialidades)
                    .ThenInclude(ce => ce.Especialidad)
                .Include(c => c.CategoriaSLAs)
                    .ThenInclude(cs => cs.SLA)
                .FirstOrDefaultAsync(c => c.id_categoria == id);
        }

        
        public async Task<Categorias> FindCategoriaByEtiquetaAsync(int idEtiqueta)
        {
            return await _context.Categorias
                .Include(c => c.CategoriaEtiquetas)
                    .ThenInclude(ce => ce.Etiqueta)
                .Include(c => c.CategoriaEspecialidades)
                    .ThenInclude(ce => ce.Especialidad)
                .Include(c => c.CategoriaSLAs)
                    .ThenInclude(cs => cs.SLA)
                .FirstOrDefaultAsync(c => c.CategoriaEtiquetas.Any(ce => ce.id_etiqueta == idEtiqueta));
        }

        public async Task<ICollection<Categorias>> ListAsync()
        {
            return await _context.Categorias
                .Include(c => c.CategoriaEtiquetas)
                    .ThenInclude(ce => ce.Etiqueta)
                .Include(c => c.CategoriaEspecialidades)
                    .ThenInclude(ce => ce.Especialidad)
                .Include(c => c.CategoriaSLAs)
                    .ThenInclude(cs => cs.SLA)
                .ToListAsync();
        }

        public async Task CreateAsync(Categorias categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Categorias categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Categorias.FindAsync(id);
            if (entity != null)
            {
                _context.Categorias.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}