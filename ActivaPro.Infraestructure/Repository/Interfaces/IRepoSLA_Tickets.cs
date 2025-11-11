using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoSLA_Tickets
    {
        Task<IEnumerable<SLA_Tickets>> ListAsync();
        Task<SLA_Tickets?> FindByIdAsync(int id);
    }
}
