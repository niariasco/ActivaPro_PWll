using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{
    public record AsignacionesDTO
    {
        public int IdAsignacion { get; set; }
        public int IdTicket { get; set; }
        public int IdUsuarioAsignado { get; set; }
        public int? IdUsuarioAsignador { get; set; }
        public string TipoAsignacion { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public string Justificacion { get; set; } // NUEVO
        public decimal? PuntajeAsignacion { get; set; } // NUEVO

        // Información del técnico
        public string NombreTecnico { get; set; }
        public string CorreoTecnico { get; set; }

        // Información del ticket
        public TicketesDTO Ticket { get; set; }

        // Cálculos adicionales
        public string SemanaAsignacion { get; set; }
        public int NumeroSemana { get; set; }
        public int Anio { get; set; }
    }

    public record TecnicoAsignacionesDTO
    {
        public int IdTecnico { get; set; }
        public string NombreTecnico { get; set; }
        public string CorreoTecnico { get; set; }
        public int TotalTicketsAsignados { get; set; }
        public int TicketsPendientes { get; set; }
        public int TicketsEnProceso { get; set; }
        public int TicketsCerrados { get; set; }
        public List<AsignacionPorSemanaDTO> AsignacionesPorSemana { get; set; }
    }

    public record AsignacionPorSemanaDTO
    {
        public int NumeroSemana { get; set; }
        public int Anio { get; set; }
        public string RangoFechas { get; set; }
        public List<TicketAsignadoDTO> Tickets { get; set; }
    }

    public record TicketAsignadoDTO
    {
        public int IdTicket { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }
        public int? HorasRestantes { get; set; }
        public string ColorUrgencia { get; set; }
        public string IconoCategoria { get; set; }
        public int PorcentajeProgreso { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }

    // NUEVOS DTOs para asignación automática y manual
    public record AsignacionAutomaticaRequestDTO
    {
        public int IdTicket { get; set; }
    }

    public record AsignacionManualRequestDTO
    {
        public int IdTicket { get; set; }
        public int IdTecnico { get; set; }
        public int IdUsuarioAsignador { get; set; }
        public string Justificacion { get; set; }
    }

    public record AsignacionResultDTO
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public AsignacionesDTO Asignacion { get; set; }
        public TecnicoSeleccionadoDTO TecnicoSeleccionado { get; set; }
        public decimal? Puntaje { get; set; }
        public string Justificacion { get; set; }
    }

    public record TecnicoSeleccionadoDTO
    {
        public int IdTecnico { get; set; }
        public string NombreTecnico { get; set; }
        public string CorreoTecnico { get; set; }
        public int CargaActual { get; set; }
        public List<string> Especialidades { get; set; }
        public bool Disponible { get; set; }
    }

    public record TecnicoDisponibleDTO
    {
        public int IdTecnico { get; set; }
        public string NombreTecnico { get; set; }
        public string CorreoTecnico { get; set; }
        public int TicketsActivos { get; set; }
        public int TicketsPendientes { get; set; }
        public int TicketsEnProceso { get; set; }
        public List<string> Especialidades { get; set; }
        public bool TieneEspecialidad { get; set; }
        public string NivelCarga { get; set; } // "Baja", "Media", "Alta"
        public bool Disponible { get; set; }
    }

    public record TicketPendienteAsignacionDTO
    {
        public int IdTicket { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaLimiteResolucion { get; set; }
        public int? HorasRestantes { get; set; }
        public int? TiempoResolucionHoras { get; set; }
        public string ColorUrgencia { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public record PuntajeAsignacionDTO
    {
        public int IdTecnico { get; set; }
        public string NombreTecnico { get; set; }
        public decimal Puntaje { get; set; }
        public int ValorPrioridad { get; set; }
        public int TiempoRestanteSLA { get; set; }
        public int CargaTrabajo { get; set; }
        public bool TieneEspecialidad { get; set; }
        public string Justificacion { get; set; }
    }
}