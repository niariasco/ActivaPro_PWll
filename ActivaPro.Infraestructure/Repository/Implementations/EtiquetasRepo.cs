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
    public class EtiquetasRepo : IRepoEtiquetas
    {
        private readonly ActivaProContext _context;

        public EtiquetasRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Etiquetas>> ListAsync()
        {
            return await _context.Etiquetas
                .ToListAsync();
        }

        public async Task<Etiquetas?> FindByIdAsync(int id)
        {
            return await _context.Etiquetas
                .FirstOrDefaultAsync(e => e.id_etiqueta == id);
        }
    }
}