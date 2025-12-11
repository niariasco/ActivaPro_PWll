using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ITicketesService
    {
        // ========== CONSULTAS ==========
        Task<TicketesDTO?> FindByIdAsync(int id);
        Task<IEnumerable<TicketesDTO>> ListAsync();
        Task<IEnumerable<TicketesDTO>> ListByRolAsync(int idUsuario, string rol);
        Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario);

        // ========== PREPARACIÓN DE DTOs ==========
        Task<TicketCreateDTO> PrepareCreateDTOAsync(int idUsuarioSolicitante);
        Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket, string rolUsuario);
        Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket);

        // ========== CREACIÓN ==========
        Task<int> CreateTicketAsync(TicketCreateDTO dto);
        Task<int> CreateTicketAsync(TicketCreateDTO dto, string rutaImagenes);

        // ========== ACTUALIZACIÓN ==========
        Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual, string rolUsuario);
        Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual);

        // ========== CAMBIO RÁPIDO DE ESTADO ==========
        /// <summary>
        /// Cambia el estado del ticket de forma rápida con comentario obligatorio
        /// </summary>
        Task CambiarEstadoRapidoAsync(int idTicket, string nuevoEstado, int idUsuarioActual, string comentario);

        // ========== CIERRE ==========
        Task CloseTicketAsync(int idTicket, int idUsuarioActual);

        // ========== GESTIÓN DE IMÁGENES ==========
        Task DeleteImagenAsync(int idImagen, string rutaFisica);
    }
}