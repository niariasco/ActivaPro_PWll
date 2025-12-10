using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepo _repo;
        private readonly INotificacionRealtimeSender _realtime;

        public NotificacionService(INotificacionRepo repo, INotificacionRealtimeSender realtime)
        {
            _repo = repo;
            _realtime = realtime;
        }

        public async Task CrearCambioEstadoTicketAsync(IEnumerable<int> usuariosDestino, int ticketId, string anterior, string nuevo, string responsable, string motivo)
        {
            var msg = $"Ticket #{ticketId}: {anterior} → {nuevo}. Resp: {responsable}. Motivo: {motivo}";
            var fecha = DateTime.Now;

            foreach (var u in usuariosDestino.Distinct())
            {
                await _repo.AddAsync(new Notificacion
                {
                    IdUsuario = u,
                    IdTicket = ticketId,
                    Accion = "TicketStateChange",
                    Mensaje = msg,
                    Leido = false,
                    FechaEnvio = fecha
                });
            }
            await _repo.SaveAsync();

            foreach (var u in usuariosDestino.Distinct())
            {
                var ultima = (await _repo.ListAsync(u, 0, 1)).FirstOrDefault();
                if (ultima != null)
                    await _realtime.SendNuevaAsync(u, ToDto(ultima));
            }
        }

        public async Task CrearLoginAsync(int usuarioId)
        {
            var n = new Notificacion
            {
                IdUsuario = usuarioId,
                IdTicket = null,
                Accion = "Login",
                Mensaje = "Inicio de sesión",
                Leido = false,
                FechaEnvio = DateTime.Now
            };
            await _repo.AddAsync(n);
            await _repo.SaveAsync();
            await _realtime.SendNuevaAsync(usuarioId, ToDto(n));
        }

        public async Task CrearLogoutAsync(int usuarioId)
        {
            var n = new Notificacion
            {
                IdUsuario = usuarioId,
                IdTicket = null,
                Accion = "Logout",
                Mensaje = "Cierre de sesión",
                Leido = false,
                FechaEnvio = DateTime.Now
            };
            await _repo.AddAsync(n);
            await _repo.SaveAsync();
            await _realtime.SendNuevaAsync(usuarioId, ToDto(n));
        }

        public async Task<IEnumerable<NotificacionDTO>> ListarAsync(int usuarioId, int skip = 0, int take = 30) =>
            (await _repo.ListAsync(usuarioId, skip, take)).Select(ToDto);

        public Task<int> NoLeidasAsync(int usuarioId) => _repo.CountUnreadAsync(usuarioId);

        public async Task<bool> MarcarLeidaAsync(int idNotificacion, int usuarioActual)
        {
            var n = await _repo.FindAsync(idNotificacion);
            if (n == null || n.IdUsuario != usuarioActual) return false;
            await _repo.MarkReadAsync(n);
            await _repo.SaveAsync();
            await _realtime.SendActualizadaAsync(usuarioActual, n.IdNotificacion, n.Leido);
            return true;
        }

        public async Task<int> MarcarTodasLeidasAsync(int usuarioId)
        {
            var afectados = await _repo.MarkAllUnreadAsync(usuarioId);
            await _repo.SaveAsync();

            // Notificar actualizaciones individuales (opcional, puedes omitir si son muchas)
            // Podrías enviar un solo evento global, pero mantengo consistencia:
            if (afectados > 0)
            {
                var recientes = await _repo.ListAsync(usuarioId, 0, afectados);
                foreach (var n in recientes.Where(x => x.Leido))
                    await _realtime.SendActualizadaAsync(usuarioId, n.IdNotificacion, true);
            }

            return afectados;
        }

        public async Task CrearEventoTicketAsync(IEnumerable<int> usuariosDestino, int ticketId, string tipoEvento, string descripcion, string responsable)
        {
            var msg = $"Ticket #{ticketId}: {tipoEvento}. {descripcion}. Resp: {responsable}";
            var fecha = DateTime.Now;

            foreach (var u in usuariosDestino.Distinct())
            {
                await _repo.AddAsync(new Notificacion
                {
                    IdUsuario = u,
                    IdTicket = ticketId,
                    Accion = "TicketEvent",   // Accion diferenciada para eventos CRUD
                    Mensaje = msg,
                    Leido = false,
                    FechaEnvio = fecha
                });
            }
            await _repo.SaveAsync();

            foreach (var u in usuariosDestino.Distinct())
            {
                var ultima = (await _repo.ListAsync(u, 0, 1)).FirstOrDefault();
                if (ultima != null)
                    await _realtime.SendNuevaAsync(u, ToDto(ultima));
            }
        }

        private static NotificacionDTO ToDto(Notificacion n) => new()
        {
            IdNotificacion = n.IdNotificacion,
            IdTicket = n.IdTicket,
            Accion = n.Accion,
            Mensaje = n.Mensaje,
            Leido = n.Leido,
            FechaEnvio = n.FechaEnvio
        };
    }
}
