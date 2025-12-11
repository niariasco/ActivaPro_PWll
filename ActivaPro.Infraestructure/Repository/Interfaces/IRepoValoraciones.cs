using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoValoraciones
    {
        // Consultas
        Task<Valoracion_Notificaciones?> FindByIdAsync(int id);
        Task<Valoracion_Notificaciones?> FindByTicketIdAsync(int idTicket);
        Task<ICollection<Valoracion_Notificaciones>> ListAsync();
        Task<ICollection<Valoracion_Notificaciones>> ListByClienteAsync(int idCliente);
        Task<ICollection<Valoracion_Notificaciones>> ListByTecnicoAsync(int idTecnico);

        // Validaciones
        Task<bool> ExisteValoracionParaTicketAsync(int idTicket);

        // Creación
        Task CreateAsync(Valoracion_Notificaciones valoracion);

        // Estadísticas
        Task<int> CountAsync();
        Task<double> GetPromedioGeneralAsync();
        Task<Dictionary<byte, int>> GetDistribucionPuntajesAsync();
    }
}