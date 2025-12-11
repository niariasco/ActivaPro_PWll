using ActivaPro.Application.DTOs;
using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface INotificacionService
    {
        Task CrearCambioEstadoTicketAsync(IEnumerable<int> usuariosDestino, int ticketId, string anterior, string nuevo, string responsable, string motivo);
        Task CrearLoginAsync(int usuarioId);
        Task CrearLogoutAsync(int usuarioId);
        Task CrearEventoTicketAsync(IEnumerable<int> usuariosDestino, int ticketId, string tipoEvento, string descripcion, string responsable);
        Task<IEnumerable<NotificacionDTO>> ListarAsync(int usuarioId, int skip = 0, int take = 30);
        Task<int> NoLeidasAsync(int usuarioId);
        Task<bool> MarcarLeidaAsync(int idNotificacion, int usuarioActual);
        Task<int> MarcarTodasLeidasAsync(int usuarioId);

        Task<Notificacion> ObtenerOCrearNotificacionParaTicketAsync(int idTicket, int idUsuario);
    }
}
