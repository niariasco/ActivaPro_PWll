using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class TecnicoRepo : IRepoTecnico
    {
        private readonly ActivaProContext _context;

        public TecnicoRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Tecnicos>> ListAsync()
        {
            return await _context.Tecnico
                .Include(t => t.Usuario)
                .ToListAsync();
        }

        public async Task<Tecnicos?> FindByIdAsync(int id)
        {
            return await _context.Tecnico
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.IdTecnico == id);
        }

        public async Task CreateAsync(Tecnicos tecnico)
        {
            _context.Tecnico.Add(tecnico);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tecnicos tecnico)
        {
            _context.Tecnico.Update(tecnico);
            await _context.SaveChangesAsync();
        }
    }
}