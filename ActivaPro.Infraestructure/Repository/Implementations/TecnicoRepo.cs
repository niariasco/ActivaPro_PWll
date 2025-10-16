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
    public class TecnicoRepo : IRepoTecnico
    {
        private readonly ActivaProContext _context;

        public TecnicoRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Tecnicos>> ListAsync()
        {
            // Solo usuarios con rol "Técnico"
            var collection = await _context.Set<Tecnicos>().ToListAsync();
            return collection;
        }

        public async Task<Tecnicos?> FindByIdAsync(int id)
        {
            return await _context.Tecnico
                      .Include(t => t.Usuario) // importante para traer Nombre y Correo
                      .FirstOrDefaultAsync(t => t.IdTecnico == id);
        }
    }
}