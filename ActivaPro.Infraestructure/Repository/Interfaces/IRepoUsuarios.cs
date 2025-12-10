using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoUsuarios
    {
        Task<Usuarios?> FindByIdAsync(int id);
        Task<Usuarios?> FindByCorreoAsync(string correo);
        Task CreateAsync(Usuarios usuario);
        Task UpdateAsync(Usuarios usuario);
        Task<ICollection<Usuarios>> ListAsync();
        Task<ICollection<Usuarios>> ListByRolAsync(string rol);
    }
}