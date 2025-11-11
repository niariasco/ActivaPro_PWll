
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
    public class EspecialidadesRepo : IRepoEspecialidades
    {
        private readonly ActivaProContext _context;

        public EspecialidadesRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Especialidades>> ListAsync()
        {
            return await _context.Especialidades
                .ToListAsync();
        }

        public async Task<Especialidades?> FindByIdAsync(int id)
        {
            return await _context.Especialidades
                .FirstOrDefaultAsync(e => e.id_especialidad == id);
        }
    }
}