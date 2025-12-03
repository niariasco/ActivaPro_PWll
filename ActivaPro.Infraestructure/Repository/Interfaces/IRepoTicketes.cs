using ActivaPro.Infraestructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoTicketes
    {
        // ========== CONSULTAS ==========
        Task<Tickets> FindByIdAsync(int id);
        Task<ICollection<Tickets>> ListAsync();
        Task<ICollection<Tickets>> ListByUsuarioSolicitanteAsync(int idUsuario);
        Task<ICollection<Tickets>> ListByUsuarioAsignadoAsync(int idUsuario);

        /// <summary>
        /// Lista tickets filtrados por estado
        /// </summary>
        /// <param name="estado">Estado del ticket (Pendiente, Asignado, En Proceso, Cerrado)</param>
        Task<ICollection<Tickets>> ListByEstadoAsync(string estado);

        // ========== CREACIÓN ==========
        Task CreateAsync(Tickets ticket);
        Task AddHistorialAsync(Historial_Tickets historial);
        Task AddImagenAsync(Imagenes_Tickets imagen);

        // ========== ACTUALIZACIÓN ==========
        /// <summary>
        /// Actualiza un ticket existente (incluye cambio de estado)
        /// </summary>
        Task UpdateAsync(Tickets ticket);

        // ========== GESTIÓN DE IMÁGENES ==========
        /// <summary>
        /// Busca una imagen específica por ID
        /// </summary>
        Task<Imagenes_Tickets> FindImagenByIdAsync(int idImagen);

        /// <summary>
        /// Elimina una imagen específica
        /// </summary>
        Task DeleteImagenAsync(int idImagen);

        // NOTA: Ya NO existe el método DeleteAsync para tickets
        // Los tickets se cierran cambiando su estado, no se eliminan
    }
}