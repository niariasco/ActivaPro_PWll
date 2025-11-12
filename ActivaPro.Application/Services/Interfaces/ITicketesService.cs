using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface ITicketesService
    {
        // ========== CONSULTAS ==========

        /// <summary>
        /// Busca un ticket por su ID
        /// </summary>
        Task<TicketesDTO?> FindByIdAsync(int id);

        /// <summary>
        /// Lista todos los tickets
        /// </summary>
        Task<IEnumerable<TicketesDTO>> ListAsync();

        /// <summary>
        /// Lista tickets según el rol del usuario
        /// - Administrador: todos los tickets
        /// - Técnico: tickets asignados
        /// - Cliente: tickets solicitados
        /// </summary>
        Task<IEnumerable<TicketesDTO>> ListByRolAsync(int idUsuario, string rol);

        /// <summary>
        /// Obtiene la información de un usuario
        /// </summary>
        Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario);

        /// <summary>
        /// Prepara el DTO para crear un nuevo ticket
        /// </summary>
        Task<TicketCreateDTO> PrepareCreateDTOAsync(int idUsuarioSolicitante);

        // ========== CREACIÓN ==========

        /// <summary>
        /// Crea un nuevo ticket sin imágenes
        /// </summary>
        Task<int> CreateTicketAsync(TicketCreateDTO dto);

        /// <summary>
        /// Crea un nuevo ticket con imágenes
        /// </summary>
        Task<int> CreateTicketAsync(TicketCreateDTO dto, string rutaImagenes);

        // ========== ACTUALIZACIÓN ==========

        /// <summary>
        /// Prepara el DTO para editar un ticket existente
        /// </summary>
        Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket);

        /// <summary>
        /// Actualiza un ticket existente
        /// </summary>
        Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual);

        // ========== CIERRE DE TICKET ==========

        /// <summary>
        /// Cierra un ticket cambiando su estado a "Cerrado"
        /// Solo permitido para Cliente (propietario) y Administrador
        /// </summary>
        /// <param name="idTicket">ID del ticket a cerrar</param>
        /// <param name="idUsuarioActual">ID del usuario que cierra el ticket</param>
        Task CloseTicketAsync(int idTicket, int idUsuarioActual);

        // ========== GESTIÓN DE IMÁGENES ==========

        /// <summary>
        /// Elimina una imagen específica del ticket
        /// </summary>
        Task DeleteImagenAsync(int idImagen, string rutaFisica);
    }
}