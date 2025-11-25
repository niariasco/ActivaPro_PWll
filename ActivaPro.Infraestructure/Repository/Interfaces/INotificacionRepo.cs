using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface INotificacionRepo
    {
        Task AddAsync(Notificacion n);
        Task<Notificacion?> FindAsync(int id);                     // int
        Task<IEnumerable<Notificacion>> ListAsync(int userId, int skip = 0, int take = 30);
        Task<int> CountUnreadAsync(int userId);
        Task MarkReadAsync(Notificacion n);
        Task<int> MarkAllUnreadAsync(int userId);
        Task SaveAsync();
    }
}
