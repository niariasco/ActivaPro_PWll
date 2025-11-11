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

        /// <summary>
        /// Obtiene la información del usuario solicitante para preparar el formulario de creación
        /// </summary>
        Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario);

        /// <summary>
        /// Prepara el DTO con información prellenada para crear un ticket
        /// </summary>
        Task<TicketCreateDTO> PrepareCreateDTOAsync(int idUsuarioSolicitante);

        /// <summary>
        /// Crea un nuevo ticket con cálculos automáticos de SLA y registro en historial
        /// </summary>
        Task<int> CreateTicketAsync(TicketCreateDTO dto);
    }
}