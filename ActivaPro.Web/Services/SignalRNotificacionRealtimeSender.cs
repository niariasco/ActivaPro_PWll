using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ActivaPro.Web.Services
{
    public class SignalRNotificacionRealtimeSender : INotificacionRealtimeSender
    {
        private readonly IHubContext<NotificacionesHub> _hub;

        public SignalRNotificacionRealtimeSender(IHubContext<NotificacionesHub> hub)
        {
            _hub = hub;
        }

        public Task SendNuevaAsync(int userId, NotificacionDTO dto) =>
            _hub.Clients.User(userId.ToString()).SendAsync("notificaciones:nueva", dto);

        public Task SendActualizadaAsync(int userId, long idNotificacion, bool leido) =>
            _hub.Clients.User(userId.ToString()).SendAsync("notificaciones:actualizada", new { IdNotificacion = idNotificacion, Leido = leido });
    }
}