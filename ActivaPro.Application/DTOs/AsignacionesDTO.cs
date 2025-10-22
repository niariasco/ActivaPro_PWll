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
}
