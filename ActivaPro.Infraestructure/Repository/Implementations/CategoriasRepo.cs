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
                .Include(c => c.CategoriaEspecialidades)
                .Include(c => c.SLA_Tickets)
                .FirstOrDefaultAsync(c => c.id_categoria == id);
        }

        public async Task<ICollection<Categorias>> ListAsync()
      {
            //Select * from Categorias
            // var collection = await _context.Set<Categorias>().ToListAsync();
            //  return collection;

            return await _context.Categorias
           .Include(c => c.CategoriaEtiquetas)     
           .Include(c => c.CategoriaEspecialidades)  
           .Include(c => c.SLA_Tickets)                     
           .ToListAsync();

        }
    }
}
