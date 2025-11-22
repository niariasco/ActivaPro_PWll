using ActivaPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface INotificacionRealtimeSender
    {
        Task SendNuevaAsync(int userId, NotificacionDTO dto);
        Task SendActualizadaAsync(int userId, long idNotificacion, bool leido);
    }
}
