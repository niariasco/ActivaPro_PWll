using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class TicketesRepo : IRepoTicketes
    {
        private readonly ActivaProContext _context;

        public TicketesRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<Tickets> FindByIdAsync(int id)
        {
            return await _context.Ticketes
                .Include(t => t.UsuarioSolicitante)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaEtiquetas)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaSLAs)
                .Include(t => t.SLA)
                .Include(t => t.Imagenes)
                .Include(t => t.Historial)
                    .ThenInclude(h => h.Usuario)
                .Include(t => t.Valoraciones)
                .FirstOrDefaultAsync(t => t.IdTicket == id);
        }

        public async Task<ICollection<Tickets>> ListAsync()
        {
            return await _context.Ticketes
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }

        public async Task<ICollection<Tickets>> ListByUsuarioSolicitanteAsync(int idUsuario)
        {
            return await _context.Ticketes
                 .Where(t => t.IdUsuarioSolicitante == idUsuario)
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }

        public async Task<ICollection<Tickets>> ListByUsuarioAsignadoAsync(int idUsuario)
        {
            return await _context.Ticketes
                 .Where(t => t.IdUsuarioAsignado == idUsuario)
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }
    }
}