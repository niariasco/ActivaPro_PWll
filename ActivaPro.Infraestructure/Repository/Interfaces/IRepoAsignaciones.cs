using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoAsignaciones
    {
        Task<ICollection<AsignacionesTickets>> ListAsync();
        Task<ICollection<AsignacionesTickets>> ListByTecnicoAsync(int idUsuario);
        Task<ICollection<AsignacionesTickets>> ListByTecnicoAndWeekAsync(int idUsuario, int semana, int anio);
        Task<AsignacionesTickets> FindByIdAsync(int id);
        Task<AsignacionesTickets> GetUltimaAsignacionByTicketAsync(int idTicket);
    }
}