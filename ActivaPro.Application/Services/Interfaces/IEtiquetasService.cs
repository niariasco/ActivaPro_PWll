using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IEtiquetasService
    {
        Task<IEnumerable<Etiquetas>> ListAsync();
        Task<Etiquetas?> FindByIdAsync(int id);

    }
}
