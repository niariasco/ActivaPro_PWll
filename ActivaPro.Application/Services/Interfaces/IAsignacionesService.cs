using ActivaPro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IAsignacionesService
    {
        // ========== ASIGNACIÓN AUTOMÁTICA (AUTOTRIAGE) ==========

        /// <summary>
        /// Asigna un ticket específico automáticamente usando el algoritmo de autotriage
        /// </summary>
        /// <param name="idTicket">ID del ticket a asignar</param>
        /// <returns>Resultado de la asignación con puntaje y justificación</returns>
        Task<AsignacionResultDTO> AsignarAutomaticamenteAsync(int idTicket);

        /// <summary>
        /// Asigna todos los tickets pendientes automáticamente
        /// </summary>
        /// <returns>Lista de resultados de cada asignación</returns>
        Task<List<AsignacionResultDTO>> AsignarTodosPendientesAsync();

        // ========== ASIGNACIÓN MANUAL ==========

        /// <summary>
        /// Asigna un ticket manualmente a un técnico específico
        /// </summary>
        /// <param name="request">Datos de la asignación manual</param>
        /// <returns>Resultado de la asignación</returns>
        Task<AsignacionResultDTO> AsignarManualmenteAsync(AsignacionManualRequestDTO request);

        // ========== CONSULTAS ==========

        /// <summary>
        /// Obtiene la lista de tickets pendientes de asignación
        /// </summary>
        /// <returns>Lista de tickets pendientes con información de urgencia</returns>
        Task<IEnumerable<TicketPendienteAsignacionDTO>> GetTicketsPendientesAsync();

        /// <summary>
        /// Obtiene la lista de técnicos disponibles con información de carga
        /// </summary>
        /// <param name="idTicket">ID del ticket para filtrar por especialidad (opcional)</param>
        /// <returns>Lista de técnicos disponibles</returns>
        Task<IEnumerable<TecnicoDisponibleDTO>> GetTecnicosDisponiblesAsync(int? idTicket = null);

        /// <summary>
        /// Obtiene las asignaciones agrupadas por técnico
        /// </summary>
        /// <returns>Lista de técnicos con sus asignaciones organizadas por semana</returns>
        Task<IEnumerable<TecnicoAsignacionesDTO>> GetAsignacionesPorTecnicoAsync();

        /// <summary>
        /// Obtiene las asignaciones de un técnico específico
        /// </summary>
        /// <param name="idTecnico">ID del técnico</param>
        /// <returns>Asignaciones del técnico organizadas por semana</returns>
        Task<TecnicoAsignacionesDTO> GetAsignacionesByTecnicoIdAsync(int idTecnico);
    }
}