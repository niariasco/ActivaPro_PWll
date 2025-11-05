using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
     public interface IEspecialidadesService
    {
        Task<IEnumerable<Especialidades>> ListAsync();
        Task<Especialidades?> FindByIdAsync(int id);

    }
}
