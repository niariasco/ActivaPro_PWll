using ActivaPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ITecnicoService
    {
        Task<ICollection<TecnicosDTO>> ListAsync();
        Task<TecnicosDTO?> FindByIdAsync(int id);
    }
}
