using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IAsignacionesService
    {
        // Métodos existentes
        Task<IEnumerable<TecnicoAsignacionesDTO>> GetAsignacionesPorTecnicoAsync();
        Task<TecnicoAsignacionesDTO> GetAsignacionesByTecnicoIdAsync(int idTecnico);
        Task<IEnumerable<AsignacionPorSemanaDTO>> GetAsignacionesPorSemanaAsync(int idTecnico);

        // NUEVOS MÉTODOS - Asignación Automática
        Task<AsignacionResultDTO> AsignarAutomaticamenteAsync(int idTicket);
        Task<IEnumerable<AsignacionResultDTO>> AsignarTodosPendientesAsync();

        // NUEVOS MÉTODOS - Asignación Manual
        Task<AsignacionResultDTO> AsignarManualmenteAsync(AsignacionManualRequestDTO request);
        Task<IEnumerable<TecnicoDisponibleDTO>> GetTecnicosDisponiblesAsync(int? idTicket = null);
        Task<IEnumerable<TicketPendienteAsignacionDTO>> GetTicketsPendientesAsync();
    }
}