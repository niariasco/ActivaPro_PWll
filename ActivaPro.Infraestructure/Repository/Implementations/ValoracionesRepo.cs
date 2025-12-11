using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class ValoracionesRepo : IRepoValoraciones
    {
        private readonly ActivaProContext _context;

        public ValoracionesRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<Valoracion_Notificaciones?> FindByIdAsync(int id)
        {
            return await _context.Valoracion_Notificaciones
                .Include(v => v.IdNotificacionNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(v => v.IdValoracion == id);
        }

        public async Task<Valoracion_Notificaciones?> FindByTicketIdAsync(int idTicket)
        {
            // Buscar la notificación asociada al ticket y luego la valoración
            return await _context.Valoracion_Notificaciones
                .Include(v => v.IdNotificacionNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Where(v => v.IdNotificacionNavigation != null &&
                           v.IdNotificacionNavigation.IdTicket == idTicket)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Valoracion_Notificaciones>> ListAsync()
        {
            return await _context.Valoracion_Notificaciones
                .Include(v => v.IdNotificacionNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .OrderByDescending(v => v.FechaValoracion)
                .ToListAsync();
        }

        public async Task<ICollection<Valoracion_Notificaciones>> ListByClienteAsync(int idCliente)
        {
            return await _context.Valoracion_Notificaciones
                .Include(v => v.IdNotificacionNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Where(v => v.IdUsuario == idCliente)
                .OrderByDescending(v => v.FechaValoracion)
                .ToListAsync();
        }

        public async Task<ICollection<Valoracion_Notificaciones>> ListByTecnicoAsync(int idTecnico)
        {
            return await _context.Valoracion_Notificaciones
                .Include(v => v.IdNotificacionNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Where(v => v.IdNotificacionNavigation != null &&
                           v.IdNotificacionNavigation.IdTicket != null &&
                           v.IdNotificacionNavigation.IdUsuario == idTecnico)
                .OrderByDescending(v => v.FechaValoracion)
                .ToListAsync();
        }

        public async Task<bool> ExisteValoracionParaTicketAsync(int idTicket)
        {
            return await _context.Valoracion_Notificaciones
                .AnyAsync(v => v.IdNotificacionNavigation != null &&
                              v.IdNotificacionNavigation.IdTicket == idTicket);
        }

        public async Task CreateAsync(Valoracion_Notificaciones valoracion)
        {
            _context.Valoracion_Notificaciones.Add(valoracion);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Valoracion_Notificaciones.CountAsync();
        }

        public async Task<double> GetPromedioGeneralAsync()
        {
            if (!await _context.Valoracion_Notificaciones.AnyAsync())
                return 0;

            return await _context.Valoracion_Notificaciones
                .AverageAsync(v => (double)v.Puntaje);
        }

        public async Task<Dictionary<byte, int>> GetDistribucionPuntajesAsync()
        {
            var distribucion = await _context.Valoracion_Notificaciones
                .GroupBy(v => v.Puntaje)
                .Select(g => new { Puntaje = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            return distribucion.ToDictionary(x => x.Puntaje, x => x.Cantidad);
        }
    }
}