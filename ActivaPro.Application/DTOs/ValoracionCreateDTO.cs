using System;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Application.DTOs
{
    /// <summary>
    /// DTO para crear una valoración (solo clientes, solo tickets cerrados)
    /// </summary>
    public class ValoracionCreateDTO
    {
        [Required(ErrorMessage = "El ID del ticket es obligatorio")]
        public int IdTicket { get; set; }

        [Required(ErrorMessage = "El puntaje es obligatorio")]
        [Range(1, 5, ErrorMessage = "El puntaje debe estar entre 1 y 5")]
        public byte Puntaje { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres")]
        public string Comentario { get; set; }

        // Información del ticket (solo lectura)
        public string? TituloTicket { get; set; }
        public string? EstadoTicket { get; set; }
        public DateTime? FechaCreacionTicket { get; set; }
        public int IdUsuarioSolicitante { get; set; }
        public int? IdNotificacion { get; set; } // Para vincular con notificación
    }

    /// <summary>
    /// DTO para mostrar una valoración completa
    /// </summary>
    public class ValoracionDTO
    {
        public int IdValoracion { get; set; }
        public int IdNotificacion { get; set; }
        public int IdUsuario { get; set; }
        public int IdTicket { get; set; }
        public byte Puntaje { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaValoracion { get; set; }

        // Información del ticket relacionado
        public string TituloTicket { get; set; }
        public string EstadoTicket { get; set; }
        public string NombreCliente { get; set; }
        public string CategoriaNombre { get; set; }
        public DateTime FechaCreacionTicket { get; set; }
        public DateTime? FechaResolucionTicket { get; set; }

        // Información del técnico asignado
        public string? NombreTecnico { get; set; }

        // Indicador de satisfacción
        public string NivelSatisfaccion => Puntaje switch
        {
            5 => "Excelente ⭐⭐⭐⭐⭐",
            4 => "Muy Bueno ⭐⭐⭐⭐",
            3 => "Bueno ⭐⭐⭐",
            2 => "Regular ⭐⭐",
            1 => "Malo ⭐",
            _ => "Sin calificación"
        };

        public string ColorSatisfaccion => Puntaje switch
        {
            5 => "success",
            4 => "info",
            3 => "warning",
            2 => "orange",
            1 => "danger",
            _ => "secondary"
        };
    }

    /// <summary>
    /// DTO para estadísticas de valoraciones
    /// </summary>
    public class ValoracionEstadisticasDTO
    {
        public int TotalValoraciones { get; set; }
        public double PromedioGeneral { get; set; }
        public int Excelente { get; set; } // 5 estrellas
        public int MuyBueno { get; set; }  // 4 estrellas
        public int Bueno { get; set; }     // 3 estrellas
        public int Regular { get; set; }   // 2 estrellas
        public int Malo { get; set; }      // 1 estrella

        public double PorcentajeExcelente => TotalValoraciones > 0 ? Math.Round((Excelente * 100.0 / TotalValoraciones), 1) : 0;
        public double PorcentajeMuyBueno => TotalValoraciones > 0 ? Math.Round((MuyBueno * 100.0 / TotalValoraciones), 1) : 0;
        public double PorcentajeBueno => TotalValoraciones > 0 ? Math.Round((Bueno * 100.0 / TotalValoraciones), 1) : 0;
        public double PorcentajeRegular => TotalValoraciones > 0 ? Math.Round((Regular * 100.0 / TotalValoraciones), 1) : 0;
        public double PorcentajeMalo => TotalValoraciones > 0 ? Math.Round((Malo * 100.0 / TotalValoraciones), 1) : 0;
    }
}