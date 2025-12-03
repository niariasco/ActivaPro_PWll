using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoAsignaciones
    {
        // Métodos de consulta existentes
        Task<ICollection<AsignacionesTickets>> ListAsync();
        Task<ICollection<AsignacionesTickets>> ListByTecnicoAsync(int idUsuario);
        Task<ICollection<AsignacionesTickets>> ListByTecnicoAndWeekAsync(int idUsuario, int semana, int anio);
        Task<AsignacionesTickets> FindByIdAsync(int id);
        Task<AsignacionesTickets> GetUltimaAsignacionByTicketAsync(int idTicket);

        // Métodos CRUD
        Task<AsignacionesTickets> AddAsync(AsignacionesTickets asignacion);
        Task<AsignacionesTickets> UpdateAsync(AsignacionesTickets asignacion);
        Task<bool> DeleteAsync(int id);

        // Métodos auxiliares para asignación
        Task<bool> ExisteAsignacionActivaAsync(int idTicket);
        Task<int> CountTicketsActivosByTecnicoAsync(int idTecnico);
        Task<int> CountTicketsPendientesByTecnicoAsync(int idTecnico);
        Task<int> CountTicketsEnProcesoByTecnicoAsync(int idTecnico);
    }
}