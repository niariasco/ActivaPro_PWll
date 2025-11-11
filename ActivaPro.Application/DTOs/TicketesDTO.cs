using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Application.DTOs
{
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

        // Historial
        public List<HistorialTicketDTO>? Historial { get; set; }

        // Valoración
        public ValoracionTicketDTO? Valoracion { get; set; }

        // Cálculos
        public int DiasDesdeCreacion { get; set; }
        public int? DiasParaResolucion { get; set; }
        public bool CumpleRespuesta { get; set; }
        public bool? CumpleResolucion { get; set; }
    }

    public record ImagenTicketDTO
    {
        public int IdImagen { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    public record HistorialTicketDTO
    {
        public int IdHistorial { get; set; }
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public DateTime FechaAccion { get; set; }
    }

    public record ValoracionTicketDTO
    {
        public int IdValoracion { get; set; }
        public int Puntaje { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaValoracion { get; set; }
    }

    // ✅ Nuevo DTO para creación de tickets
    public class TicketCreateDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una etiqueta")]
        public int IdEtiqueta { get; set; }

        // Campos automáticos (no editables)
        public int IdUsuarioSolicitante { get; set; }
        public string NombreSolicitante { get; set; }
        public string CorreoSolicitante { get; set; }
        public int? IdCategoria { get; set; }
        public string CategoriaNombre { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";

        // Información del SLA (se calculará automáticamente)
        public int? IdSLA { get; set; }
        public string SLA_Prioridad { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }
    }
}
