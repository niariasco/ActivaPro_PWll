using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoTecnico
    {
        Task<ICollection<Tecnicos>> ListAsync();
        Task<Tecnicos?> FindByIdAsync(int id);

        Task CreateAsync(Tecnicos tecnico);
        Task UpdateAsync(Tecnicos tecnico);
    }
}
