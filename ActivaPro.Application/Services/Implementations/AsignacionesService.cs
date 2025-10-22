using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class AsignacionesService : IAsignacionesService
    {
        private readonly IRepoAsignaciones _repository;
        private readonly IMapper _mapper;

        public AsignacionesService(IRepoAsignaciones repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TecnicoAsignacionesDTO>> GetAsignacionesPorTecnicoAsync()
        {
            var asignaciones = await _repository.ListAsync();

            var tecnicos = asignaciones
                .GroupBy(a => a.IdUsuarioAsignado)
                .Select(g => new TecnicoAsignacionesDTO
                {
                    IdTecnico = g.Key,
                    NombreTecnico = g.First().IdUsuarioAsignadoNavigation?.Nombre ?? "Sin nombre",
                    CorreoTecnico = g.First().IdUsuarioAsignadoNavigation?.Correo ?? "",
                    TotalTicketsAsignados = g.Select(a => a.IdTicket).Distinct().Count(),
                    TicketsPendientes = g.Count(a => a.IdTicketNavigation?.Estado == "Pendiente"),
                    TicketsEnProceso = g.Count(a => a.IdTicketNavigation?.Estado == "En Proceso"),
                    TicketsCerrados = g.Count(a => a.IdTicketNavigation?.Estado == "Cerrado"),
                    AsignacionesPorSemana = OrganizarPorSemana(g.ToList())
                })
                .ToList();

            return tecnicos;
        }

        public async Task<TecnicoAsignacionesDTO> GetAsignacionesByTecnicoIdAsync(int idTecnico)
        {
            var asignaciones = await _repository.ListByTecnicoAsync(idTecnico);

            if (!asignaciones.Any())
                return null;

            return new TecnicoAsignacionesDTO
            {
                IdTecnico = idTecnico,
                NombreTecnico = asignaciones.First().IdUsuarioAsignadoNavigation?.Nombre ?? "Sin nombre",
                CorreoTecnico = asignaciones.First().IdUsuarioAsignadoNavigation?.Correo ?? "",
                TotalTicketsAsignados = asignaciones.Select(a => a.IdTicket).Distinct().Count(),
                TicketsPendientes = asignaciones.Count(a => a.IdTicketNavigation?.Estado == "Pendiente"),
                TicketsEnProceso = asignaciones.Count(a => a.IdTicketNavigation?.Estado == "En Proceso"),
                TicketsCerrados = asignaciones.Count(a => a.IdTicketNavigation?.Estado == "Cerrado"),
                AsignacionesPorSemana = OrganizarPorSemana(asignaciones.ToList())
            };
        }

        public async Task<IEnumerable<AsignacionPorSemanaDTO>> GetAsignacionesPorSemanaAsync(int idTecnico)
        {
            var asignaciones = await _repository.ListByTecnicoAsync(idTecnico);
            return OrganizarPorSemana(asignaciones.ToList());
        }

        private List<AsignacionPorSemanaDTO> OrganizarPorSemana(List<ActivaPro.Infraestructure.Models.AsignacionesTickets> asignaciones)
        {
            var semanas = asignaciones
                .Where(a => a.FechaAsignacion.HasValue)
                .GroupBy(a => new
                {
                    Semana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        a.FechaAsignacion.Value,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday),
                    Anio = a.FechaAsignacion.Value.Year
                })
                .Select(g => new AsignacionPorSemanaDTO
                {
                    NumeroSemana = g.Key.Semana,
                    Anio = g.Key.Anio,
                    RangoFechas = ObtenerRangoSemana(g.Key.Anio, g.Key.Semana),
                    Tickets = g.Select(a => MapearTicketAsignado(a)).ToList()
                })
                .OrderByDescending(s => s.Anio)
                .ThenByDescending(s => s.NumeroSemana)
                .ToList();

            return semanas;
        }

        private TicketAsignadoDTO MapearTicketAsignado(ActivaPro.Infraestructure.Models.AsignacionesTickets asignacion)
        {
            var ticket = asignacion.IdTicketNavigation;
            var horasRestantes = CalcularHorasRestantes(ticket?.FechaLimiteResolucion);

            return new TicketAsignadoDTO
            {
                IdTicket = ticket?.IdTicket ?? 0,
                Titulo = ticket?.Titulo ?? "Sin título",
                Categoria = ticket?.Categoria?.nombre_categoria ?? "Sin categoría",
                Estado = ticket?.Estado ?? "Pendiente",
                Prioridad = ticket?.SLA?.prioridad ?? "Media",
                FechaLimiteResolucion = ticket?.FechaLimiteResolucion,
                HorasRestantes = horasRestantes,
                ColorUrgencia = DeterminarColorUrgencia(horasRestantes, ticket?.Estado),
                IconoCategoria = ObtenerIconoCategoria(ticket?.Categoria?.nombre_categoria),
                PorcentajeProgreso = CalcularPorcentajeProgreso(ticket?.Estado),
                FechaAsignacion = asignacion.FechaAsignacion ?? DateTime.Now
            };
        }

        private int? CalcularHorasRestantes(DateTime? fechaLimite)
        {
            if (!fechaLimite.HasValue)
                return null;

            var diferencia = fechaLimite.Value - DateTime.Now;
            return diferencia.TotalHours > 0 ? (int)diferencia.TotalHours : 0;
        }

        private string DeterminarColorUrgencia(int? horasRestantes, string estado)
        {
            if (estado == "Cerrado")
                return "success"; // Verde

            if (!horasRestantes.HasValue)
                return "secondary"; // Gris

            if (horasRestantes <= 6)
                return "danger"; // Rojo

            if (horasRestantes <= 24)
                return "warning"; // Amarillo

            return "info"; // Azul
        }

        private string ObtenerIconoCategoria(string categoria)
        {
            return categoria switch
            {
                "Solicitar Inventario" => "bi-box-seam",
                "Confirmacion de Pedido" => "bi-check-circle",
                "Consultar Lote" => "bi-search",
                "Problemas en Inventario" => "bi-exclamation-triangle",
                "Satisfacción y experiencia" => "bi-star",
                _ => "bi-ticket"
            };
        }

        private int CalcularPorcentajeProgreso(string estado)
        {
            return estado switch
            {
                "Pendiente" => 0,
                "En Proceso" => 50,
                "Cerrado" => 100,
                _ => 0
            };
        }

        private string ObtenerRangoSemana(int anio, int semana)
        {
            var primerDia = PrimerDiaDeSemana(anio, semana);
            var ultimoDia = primerDia.AddDays(6);
            return $"{primerDia:dd/MM} - {ultimoDia:dd/MM/yyyy}";
        }

        private DateTime PrimerDiaDeSemana(int anio, int semana)
        {
            var primerDiaDelAnio = new DateTime(anio, 1, 1);
            var diasOffset = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - primerDiaDelAnio.DayOfWeek;
            var primerLunes = primerDiaDelAnio.AddDays(diasOffset);
            return primerLunes.AddDays((semana - 1) * 7);
        }
    }
}