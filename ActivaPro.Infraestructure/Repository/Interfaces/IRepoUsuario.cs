using ActivaPro.Infraestructure.Models;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoUsuarios
    {
        Task<Usuarios?> FindByIdAsync(int id);
    }
}