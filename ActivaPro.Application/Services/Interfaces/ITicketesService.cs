using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ITicketesService
    {
        Task<IEnumerable<TicketesDTO>> ListAsync();
        Task<TicketesDTO?> FindByIdAsync(int id);

        
        Task<IEnumerable<TicketesDTO>> ListByRolAsync(int idUsuario, string rol);
    }
}