using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoTicketes
    {
        // Consultas
        Task<Tickets> FindByIdAsync(int id);
        Task<ICollection<Tickets>> ListAsync();
        Task<ICollection<Tickets>> ListByUsuarioSolicitanteAsync(int idUsuario);
        Task<ICollection<Tickets>> ListByUsuarioAsignadoAsync(int idUsuario);
        Task<ICollection<Tickets>> ListByEstadoAsync(string estado);

        // Creación y actualización
        Task CreateAsync(Tickets ticket);
        Task UpdateAsync(Tickets ticket);

        // ⭐ CAMBIADO: Usar Historial_Tickets (con guion bajo)
        Task AddHistorialAsync(Historial_Tickets historial);
        Task AddImagenAsync(Imagenes_Tickets imagen);

        // Gestión de imágenes
        Task<Imagenes_Tickets> FindImagenByIdAsync(int idImagen);
        Task DeleteImagenAsync(int idImagen);
        
          
            Task<ICollection<Historial_Tickets>> GetHistorialByTicketIdAsync(int idTicket);
        
    }
}