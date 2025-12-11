using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IValoracionesService
    {
        // ========== CONSULTAS ==========

        /// <summary>
        /// Busca una valoración por su ID
        /// </summary>
        Task<ValoracionDTO?> FindByIdAsync(int id);

        /// <summary>
        /// Busca una valoración asociada a un ticket específico
        /// </summary>
        Task<ValoracionDTO?> FindByTicketIdAsync(int idTicket);

        /// <summary>
        /// Lista todas las valoraciones (solo administradores)
        /// </summary>
        Task<IEnumerable<ValoracionDTO>> ListAsync();

        /// <summary>
        /// Lista valoraciones según el rol del usuario:
        /// - Cliente: solo sus propias valoraciones
        /// - Técnico: valoraciones de tickets asignados a él
        /// - Administrador: todas las valoraciones
        /// </summary>
        Task<IEnumerable<ValoracionDTO>> ListByRolAsync(int idUsuario, string rol);

        // ========== PREPARACIÓN DE DTOs ==========

        /// <summary>
        /// Prepara el DTO para crear una nueva valoración
        /// Carga información del ticket y valida que se puede valorar
        /// </summary>
        Task<ValoracionCreateDTO> PrepareCreateDTOAsync(int idTicket, int idCliente);

        // ========== VALIDACIONES ==========

        /// <summary>
        /// Valida que se puede crear una valoración para el ticket:
        /// 1. El ticket existe
        /// 2. El cliente es el solicitante del ticket
        /// 3. El ticket está cerrado
        /// 4. No existe una valoración previa (evitar duplicidad)
        /// </summary>
        /// <returns>Tupla (esValido, mensaje de error si no es válido)</returns>
        Task<(bool esValido, string mensaje)> ValidarCreacionValoracionAsync(int idTicket, int idCliente);

        // ========== CREACIÓN ==========

        /// <summary>
        /// Crea una nueva valoración:
        /// - Valida que cumple todos los requisitos
        /// - Crea o vincula con notificación existente
        /// - Registra en historial del ticket
        /// - Notifica al técnico asignado
        /// </summary>
        /// <returns>ID de la valoración creada</returns>
        Task<int> CreateValoracionAsync(ValoracionCreateDTO dto, int idCliente);

        // ========== ESTADÍSTICAS ==========

        /// <summary>
        /// Obtiene estadísticas generales de todas las valoraciones
        /// (solo para administradores)
        /// </summary>
        Task<ValoracionEstadisticasDTO> GetEstadisticasAsync();

        /// <summary>
        /// Obtiene estadísticas de valoraciones de un técnico específico
        /// (para que el técnico vea su desempeño)
        /// </summary>
        Task<ValoracionEstadisticasDTO> GetEstadisticasByTecnicoAsync(int idTecnico);
    }
}