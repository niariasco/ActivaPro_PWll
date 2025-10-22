using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoTicketes
    {
        Task<ICollection<Tickets>> ListAsync();
        Task<Tickets> FindByIdAsync(int id);

        Task<ICollection<Tickets>> ListByUsuarioSolicitanteAsync(int idUsuario);
        Task<ICollection<Tickets>> ListByUsuarioAsignadoAsync(int idUsuario);
    }
}