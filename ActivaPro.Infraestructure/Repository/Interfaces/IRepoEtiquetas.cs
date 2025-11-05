using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoEtiquetas
    {
        Task<IEnumerable<Etiquetas>> ListAsync();
        Task<Etiquetas?> FindByIdAsync(int id);
    }
}