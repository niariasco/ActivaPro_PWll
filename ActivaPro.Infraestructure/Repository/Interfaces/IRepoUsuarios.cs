using ActivaPro.Infraestructure.Models;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoUsuarios
    {
        Task<Usuarios?> FindByIdAsync(int id);
        Task<Usuarios?> FindByCorreoAsync(string correo);
        Task CreateAsync(Usuarios usuario);
        Task UpdateAsync(Usuarios usuario);
    }
}