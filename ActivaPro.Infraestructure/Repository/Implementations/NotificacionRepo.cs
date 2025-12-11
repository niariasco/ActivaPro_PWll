using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class NotificacionRepo : INotificacionRepo
    {
        private readonly ActivaProContext _ctx;
        public NotificacionRepo(ActivaProContext ctx) => _ctx = ctx;

        public async Task AddAsync(Notificacion n) => await _ctx.Notificaciones.AddAsync(n);

        public async Task<Notificacion?> FindAsync(int id) =>
            await _ctx.Notificaciones.FirstOrDefaultAsync(x => x.IdNotificacion == id);

        public async Task<IEnumerable<Notificacion>> ListAsync(int userId, int skip = 0, int take = 30) =>
            await _ctx.Notificaciones
                .Where(n => n.IdUsuario == userId)
                .OrderByDescending(n => n.FechaEnvio)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<int> CountUnreadAsync(int userId) =>
            await _ctx.Notificaciones.CountAsync(n => n.IdUsuario == userId && !n.Leido);

        public Task MarkReadAsync(Notificacion n) { n.Leido = true; return Task.CompletedTask; }

        public async Task SaveAsync() => await _ctx.SaveChangesAsync();

        public async Task<int> MarkAllUnreadAsync(int userId)
        {
            var list = await _ctx.Notificaciones
                .Where(n => n.IdUsuario == userId && !n.Leido)
                .ToListAsync();
            foreach (var n in list) n.Leido = true;
            return list.Count;
        }
        public async Task<ICollection<Notificacion>> ListByTicketAsync(int idTicket)
        {
            return await _ctx.Notificaciones
                .Where(n => n.IdTicket == idTicket)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();
        }
    }
}
