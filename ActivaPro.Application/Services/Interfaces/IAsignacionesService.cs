using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IAsignacionesService
    {
        Task<IEnumerable<TecnicoAsignacionesDTO>> GetAsignacionesPorTecnicoAsync();
        Task<TecnicoAsignacionesDTO> GetAsignacionesByTecnicoIdAsync(int idTecnico);
        Task<IEnumerable<AsignacionPorSemanaDTO>> GetAsignacionesPorSemanaAsync(int idTecnico);
    }
}