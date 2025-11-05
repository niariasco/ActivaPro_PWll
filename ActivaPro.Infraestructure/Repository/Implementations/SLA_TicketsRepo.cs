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
    public class SLA_TicketsRepo : IRepoSLA_Tickets
    {
        private readonly ActivaProContext _context;

        public SLA_TicketsRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SLA_Tickets>> ListAsync()
        {
            return await _context.SLA_Tickets
                .Include(s => s.Categoria)
                .ToListAsync();
        }

        public async Task<SLA_Tickets?> FindByIdAsync(int id)
        {
            return await _context.SLA_Tickets
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.id_sla == id);
        }
    }
}
