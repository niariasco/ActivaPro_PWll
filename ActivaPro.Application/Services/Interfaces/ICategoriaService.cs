using ActivaPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriasDTO>> ListAsync();
        Task<CategoriasDTO?> FindByIdAsync(int id);
        Task CreateAsync(CategoriasDTO dto);
        Task UpdateAsync(CategoriasDTO dto);

    }
}
