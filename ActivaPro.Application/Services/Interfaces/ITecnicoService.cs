using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ITecnicoService
    {
        Task<ICollection<TecnicosDTO>> ListAsync();
        Task<TecnicosDTO?> FindByIdAsync(int id);
        Task CreateAsync(TecnicosDTO dto);
        Task UpdateAsync(TecnicosDTO dto);

        // Catálogo para dropdown desde EspecialidadesU
        Task<List<(int Id, string Nombre)>> GetEspecialidadesUCatalogAsync();
    }
}
