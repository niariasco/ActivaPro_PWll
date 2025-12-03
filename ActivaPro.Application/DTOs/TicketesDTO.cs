using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Application.DTOs
{
    /// <summary>
    /// DTO para mostrar información completa de un ticket
    /// </summary>
    public record TicketesDTO
    {
        public int IdTicket { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int IdUsuarioSolicitante { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public string Estado { get; set; }
        public int? IdValoracion { get; set; }
        public int? IdCategoria { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public int? IdSLA { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }

        // Relaciones opcionales
        public string? CategoriaNombre { get; set; }
        public string? SLA_Descripcion { get; set; }
        public string? SLA_Prioridad { get; set; }
        public int? SLA_TiempoRespuestaHoras { get; set; }
        public int? SLA_TiempoResolucionHoras { get; set; }

        // Listas relacionadas 
        public List<string>? Etiquetas { get; set; }
        public List<ImagenTicketDTO>? Imagenes { get; set; }

        // Info de usuarios 
        public string? NombreSolicitante { get; set; }
        public string? NombreAsignado { get; set; }

        // Historial - ACTUALIZADO para soportar historial detallado
        public List<HistorialTicketDetalladoDTO>? Historial { get; set; }

        // Valoración
        public ValoracionTicketDTO? Valoracion { get; set; }

        // Cálculos
        public int DiasDesdeCreacion { get; set; }
        public int? DiasParaResolucion { get; set; }
        public bool CumpleRespuesta { get; set; }
        public bool? CumpleResolucion { get; set; }
    }

    /// <summary>
    /// DTO para imágenes adjuntas al ticket
    /// </summary>
    public record ImagenTicketDTO
    {
        public int IdImagen { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    /// <summary>
    /// DTO SIMPLE para el historial (retrocompatibilidad)
    /// </summary>
    public record HistorialTicketDTO
    {
        public int IdHistorial { get; set; }
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public DateTime FechaAccion { get; set; }
    }

    /// <summary>
    /// DTO EXTENDIDO para el historial con estado anterior, nuevo, comentario e imágenes
    /// </summary>
    public record HistorialTicketDetalladoDTO
    {
        public int IdHistorial { get; set; }
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaAccion { get; set; }
        public List<ImagenTicketDTO>? ImagenesEvidencia { get; set; }
    }

    /// <summary>
    /// DTO para la valoración del ticket
    /// </summary>
    public record ValoracionTicketDTO
    {
        public int IdValoracion { get; set; }
        public int Puntaje { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaValoracion { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo ticket
    /// Incluye validaciones y campos calculados automáticamente
    /// </summary>
    public class TicketCreateDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 150 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una etiqueta")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una etiqueta válida")]
        public int IdEtiqueta { get; set; }

        // ========== IMÁGENES ADJUNTAS ==========
        public List<IFormFile>? ImagenesAdjuntas { get; set; }

        // ========== CAMPOS AUTOMÁTICOS ==========
        public int IdUsuarioSolicitante { get; set; }
        public string? NombreSolicitante { get; set; }
        public string? CorreoSolicitante { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";

        // ========== CAMPOS CALCULADOS ==========
        public int? IdCategoria { get; set; }
        public string? CategoriaNombre { get; set; }
        public int? IdSLA { get; set; }
        public string? SLA_Prioridad { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }
    }

    /// <summary>
    /// DTO para editar un ticket existente
    /// </summary>
    public class TicketEditDTO
    {
        public int IdTicket { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 150 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; }

        // Usuario asignado (nullable - puede no estar asignado)
        public int? IdUsuarioAsignado { get; set; }
        public string? NombreUsuarioAsignado { get; set; }

        // ========== IMÁGENES ==========
        /// <summary>
        /// Nuevas imágenes a agregar
        /// </summary>
        public List<IFormFile>? NuevasImagenes { get; set; }

        /// <summary>
        /// IDs de imágenes existentes a eliminar
        /// </summary>
        public List<int>? ImagenesAEliminar { get; set; }

        /// <summary>
        /// Imágenes existentes (solo lectura)
        /// </summary>
        public List<ImagenTicketDTO>? ImagenesExistentes { get; set; }

        // ========== INFORMACIÓN NO EDITABLE ==========
        public int IdUsuarioSolicitante { get; set; }
        public string? NombreSolicitante { get; set; }
        public string? CorreoSolicitante { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }

        // Categoría y SLA (no editables directamente)
        public int? IdCategoria { get; set; }
        public string? CategoriaNombre { get; set; }
        public int? IdSLA { get; set; }
        public string? SLA_Descripcion { get; set; }
        public string? SLA_Prioridad { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }

        // Lista de estados disponibles
        public List<string> EstadosDisponibles { get; set; } = new List<string>
        {
            "Pendiente",
            "En Proceso",
            "En Espera",
            "Resuelto",
            "Cerrado",
            "Cancelado"
        };

        // Lista de técnicos disponibles para asignación
        public List<UsuarioDTO>? TecnicosDisponibles { get; set; }
    }

    /// <summary>
    /// DTO para cambio de estado del ticket con validaciones estrictas
    /// Incluye: comentario obligatorio, imagen obligatoria, validación de flujo
    /// </summary>
    public class TicketStateTransitionDTO
    {
        public int IdTicket { get; set; }

        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        public string NuevoEstado { get; set; }

        [Required(ErrorMessage = "Debe proporcionar un comentario obligatorio que justifique el cambio")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres")]
        public string Comentario { get; set; }

        [Required(ErrorMessage = "Debe adjuntar al menos una imagen como evidencia")]
        [MinLength(1, ErrorMessage = "Debe adjuntar al menos una imagen como evidencia")]
        public List<IFormFile>? ImagenesEvidencia { get; set; }

        // ========== INFORMACIÓN DE CONTEXTO (solo lectura) ==========
        public string EstadoActual { get; set; }
        public string Titulo { get; set; }
        public int IdUsuarioSolicitante { get; set; }
        public string NombreSolicitante { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public string NombreUsuarioAsignado { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Estados disponibles según flujo estricto
        public List<string> EstadosDisponibles { get; set; } = new List<string>();
    }
}