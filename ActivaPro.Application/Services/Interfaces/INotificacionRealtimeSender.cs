using ActivaPro.Application.DTOs;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface INotificacionRealtimeSender
    {
        Task SendNuevaAsync(int userId, NotificacionDTO dto);
        Task SendActualizadaAsync(int userId, int idNotificacion, bool leido);
    }
}
