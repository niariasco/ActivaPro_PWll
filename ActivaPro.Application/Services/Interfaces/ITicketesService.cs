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

        // ========== PREPARACIÓN DE DTOs ==========
        /// <summary>
        /// Prepara el DTO para crear un nuevo ticket
        /// </summary>
        Task<TicketCreateDTO> PrepareCreateDTOAsync(int idUsuarioSolicitante);

        /// <summary>
        /// Prepara el DTO para editar un ticket existente (sin rol - por defecto)
        /// </summary>
        Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket);

        /// <summary>
        /// ⭐ NUEVO: Prepara el DTO para editar un ticket existente (con rol del usuario)
        /// </summary>
        Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket, string rolUsuario);

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
        /// Actualiza un ticket existente (sin especificar rol - por defecto)
        /// </summary>
        Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual);

        /// <summary>
        /// ⭐ NUEVO: Actualiza un ticket existente (con rol del usuario para validaciones)
        /// </summary>
        Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual, string rolUsuario);

        // ========== ⭐ CAMBIO RÁPIDO DE ESTADO ==========
        /// <summary>
        /// Cambia el estado del ticket de forma rápida siguiendo el flujo secuencial
        /// SOLO permite avanzar al SIGUIENTE estado en el flujo:
        /// - Desde Pendiente/Asignado → En Proceso
        /// - Desde En Proceso → Resuelto
        /// - Desde Resuelto → No puede avanzar (solo Admin/Cliente cierran)
        /// 
        /// REGLAS DE VALIDACIÓN:
        /// - El ticket debe estar asignado al técnico actual
        /// - No se puede cambiar si está Cerrado o Cancelado
        /// - Debe seguir el flujo estrictamente (no saltar estados)
        /// - Registra historial automáticamente
        /// - Envía notificaciones a usuarios relevantes
        /// </summary>
        /// <param name="idTicket">ID del ticket a actualizar</param>
        /// <param name="nuevoEstado">Nuevo estado: debe ser el siguiente en el flujo</param>
        /// <param name="idUsuarioActual">ID del técnico que realiza el cambio</param>
        /// <exception cref="KeyNotFoundException">Si el ticket no existe</exception>
        /// <exception cref="InvalidOperationException">Si el flujo no es válido o no cumple validaciones</exception>
        Task CambiarEstadoRapidoAsync(int idTicket, string nuevoEstado, int idUsuarioActual, string comentario);

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