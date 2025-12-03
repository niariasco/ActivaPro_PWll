using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly IRepoTicketes _ticketRepository;
        private readonly IRepoUsuarios _usuarioRepository;
        private readonly IRepoSLA_Tickets _slaRepository;
        private readonly INotificacionService _notificacionService;
        private readonly IMapper _mapper;

        public AsignacionesService(
            IRepoAsignaciones repository,
            IRepoTicketes ticketRepository,
            IRepoUsuarios usuarioRepository,
            IRepoSLA_Tickets slaRepository,
            INotificacionService notificacionService,
            IMapper mapper)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _usuarioRepository = usuarioRepository;
            _slaRepository = slaRepository;
            _notificacionService = notificacionService;
            _mapper = mapper;
        }

        #region Métodos Existentes

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
                    TicketsPendientes = g.Count(a => a.IdTicketNavigation?.Estado == "Asignado"),
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
                TicketsPendientes = asignaciones.Count(a => a.IdTicketNavigation?.Estado == "Asignado"),
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

        #endregion

        #region Asignación Automática (Autotriage)

        public async Task<AsignacionResultDTO> AsignarAutomaticamenteAsync(int idTicket)
        {
            try
            {
                // 1. Validar que el ticket existe y está pendiente
                var ticket = await _ticketRepository.FindByIdAsync(idTicket);
                if (ticket == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El ticket no existe."
                    };
                }

                if (ticket.Estado != "Pendiente")
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = $"El ticket debe estar en estado 'Pendiente'. Estado actual: {ticket.Estado}"
                    };
                }

                // 2. Obtener técnicos usando ListByRolAsync
                var tecnicos = await _usuarioRepository.ListByRolAsync("Técnico");

                if (!tecnicos.Any())
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "No hay técnicos disponibles en el sistema."
                    };
                }

                // 3. Calcular puntajes para cada técnico
                var puntajes = new List<PuntajeAsignacionDTO>();
                foreach (var tecnico in tecnicos)
                {
                    var puntaje = await CalcularPuntajeAsync(tecnico.IdUsuario, ticket);
                    puntajes.Add(puntaje);
                }

                // 4. Seleccionar el técnico con mayor puntaje
                var mejorTecnico = puntajes.OrderByDescending(p => p.Puntaje).First();

                // 5. Actualizar estado del ticket PRIMERO
                ticket.Estado = "Asignado";
                ticket.IdUsuarioAsignado = mejorTecnico.IdTecnico;
                ticket.FechaActualizacion = DateTime.Now;
                await _ticketRepository.UpdateAsync(ticket);

                // 6. Crear la asignación DESPUÉS
                var asignacion = new AsignacionesTickets
                {
                    IdTicket = idTicket,
                    IdUsuarioAsignado = mejorTecnico.IdTecnico,
                    IdUsuarioAsignador = null, // Sistema - NULL es válido
                    TipoAsignacion = "Automatica",
                    FechaAsignacion = DateTime.Now,
                    PuntajeAsignacion = mejorTecnico.Puntaje,
                    Justificacion = mejorTecnico.Justificacion
                };

                await _repository.AddAsync(asignacion);

                // 7. Registrar en historial
                var historial = new Historial_Tickets
                {
                    IdTicket = idTicket,
                    IdUsuario = mejorTecnico.IdTecnico,
                    Accion = $"Asignación automática (Autotriage): {mejorTecnico.Justificacion}",
                    FechaAccion = DateTime.Now
                };
                await _ticketRepository.AddHistorialAsync(historial);

                // 8. Enviar notificaciones (con manejo de errores)
                try
                {
                    var usuariosDestino = new List<int> { ticket.IdUsuarioSolicitante, mejorTecnico.IdTecnico };
                    await _notificacionService.CrearCambioEstadoTicketAsync(
                        usuariosDestino,
                        ticket.IdTicket,
                        "Pendiente",
                        "Asignado",
                        "Sistema",
                        $"Asignación automática a {mejorTecnico.NombreTecnico}"
                    );
                }
                catch (Exception notifEx)
                {
                    // Log pero no fallar la asignación
                    Console.WriteLine($"Error al enviar notificación: {notifEx.Message}");
                }

                // 9. Recargar la asignación con sus relaciones
                var asignacionCompleta = await _repository.FindByIdAsync(asignacion.IdAsignacion);

                return new AsignacionResultDTO
                {
                    Exitoso = true,
                    Mensaje = "Asignación automática realizada exitosamente.",
                    Asignacion = asignacionCompleta != null ? _mapper.Map<AsignacionesDTO>(asignacionCompleta) : null,
                    Puntaje = mejorTecnico.Puntaje,
                    Justificacion = mejorTecnico.Justificacion,
                    TecnicoSeleccionado = new TecnicoSeleccionadoDTO
                    {
                        IdTecnico = mejorTecnico.IdTecnico,
                        NombreTecnico = mejorTecnico.NombreTecnico,
                        CargaActual = mejorTecnico.CargaTrabajo,
                        Disponible = true
                    }
                };
            }
            catch (Exception ex)
            {
                return new AsignacionResultDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error al asignar automáticamente: {ex.Message} | Inner: {ex.InnerException?.Message}"
                };
            }
        }

        public async Task<IEnumerable<AsignacionResultDTO>> AsignarTodosPendientesAsync()
        {
            var ticketsPendientes = await _ticketRepository.ListByEstadoAsync("Pendiente");
            var resultados = new List<AsignacionResultDTO>();

            foreach (var ticket in ticketsPendientes)
            {
                var resultado = await AsignarAutomaticamenteAsync(ticket.IdTicket);
                resultados.Add(resultado);
            }

            return resultados;
        }

        private async Task<PuntajeAsignacionDTO> CalcularPuntajeAsync(int idTecnico, Tickets ticket)
        {
            // 1. Obtener información del técnico
            var tecnico = await _usuarioRepository.FindByIdAsync(idTecnico);
            var cargaTrabajo = await _repository.CountTicketsActivosByTecnicoAsync(idTecnico);

            // 2. Calcular tiempo restante del SLA
            int tiempoRestanteSLA = 0;
            if (ticket.FechaLimiteResolucion.HasValue)
            {
                var diferencia = ticket.FechaLimiteResolucion.Value - DateTime.Now;
                tiempoRestanteSLA = diferencia.TotalHours > 0 ? (int)diferencia.TotalHours : 0;
            }
            else if (ticket.SLA != null && ticket.SLA.tiempo_resolucion_horas.HasValue)
            {
                tiempoRestanteSLA = ticket.SLA.tiempo_resolucion_horas.Value;
            }

            // 3. Convertir prioridad a valor numérico
            int valorPrioridad = ticket.SLA?.prioridad switch
            {
                "Crítica" => 4,
                "Alta" => 3,
                "Media" => 2,
                "Baja" => 1,
                _ => 2
            };

            // 4. Verificar especialidad (simplificado)
            bool tieneEspecialidad = true;

            // 5. Calcular puntaje según fórmula
            decimal puntaje = (valorPrioridad * 1000) - tiempoRestanteSLA - (cargaTrabajo * 50);

            // Bonus por especialidad
            if (tieneEspecialidad)
            {
                puntaje += 100;
            }

            // 6. Generar justificación detallada
            var justificacion = $"Prioridad: {ticket.SLA?.prioridad ?? "Media"} ({valorPrioridad * 1000} pts) | " +
                              $"Tiempo restante SLA: {tiempoRestanteSLA}h (-{tiempoRestanteSLA} pts) | " +
                              $"Carga actual: {cargaTrabajo} tickets (-{cargaTrabajo * 50} pts) | " +
                              $"Especialidad: {(tieneEspecialidad ? "+100 pts" : "0 pts")} | " +
                              $"Puntaje final: {puntaje}";

            return new PuntajeAsignacionDTO
            {
                IdTecnico = idTecnico,
                NombreTecnico = tecnico?.Nombre ?? "Sin nombre",
                Puntaje = puntaje,
                ValorPrioridad = valorPrioridad,
                TiempoRestanteSLA = tiempoRestanteSLA,
                CargaTrabajo = cargaTrabajo,
                TieneEspecialidad = tieneEspecialidad,
                Justificacion = justificacion
            };
        }

        #endregion

        #region Asignación Manual

        public async Task<AsignacionResultDTO> AsignarManualmenteAsync(AsignacionManualRequestDTO request)
        {
            try
            {
                // 1. Validar que el ticket existe y está pendiente
                var ticket = await _ticketRepository.FindByIdAsync(request.IdTicket);
                if (ticket == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El ticket no existe."
                    };
                }

                if (ticket.Estado != "Pendiente")
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = $"Solo se pueden asignar tickets en estado 'Pendiente'. Estado actual: {ticket.Estado}"
                    };
                }

                // 2. Validar que el técnico existe y tiene el rol correcto
                var tecnico = await _usuarioRepository.FindByIdAsync(request.IdTecnico);
                if (tecnico == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El técnico seleccionado no existe."
                    };
                }

                bool esTecnico = tecnico.UsuarioRoles != null && tecnico.UsuarioRoles.Any(ur =>
                    ur.Rol != null && (ur.Rol.NombreRol.ToLower() == "tecnico" ||
                    ur.Rol.NombreRol.ToLower() == "técnico"));

                if (!esTecnico)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El usuario seleccionado no tiene el rol de técnico."
                    };
                }

                // 3. Actualizar estado del ticket PRIMERO
                ticket.Estado = "Asignado";
                ticket.IdUsuarioAsignado = request.IdTecnico;
                ticket.FechaActualizacion = DateTime.Now;
                await _ticketRepository.UpdateAsync(ticket);

                // 4. Crear la asignación DESPUÉS
                var asignacion = new AsignacionesTickets
                {
                    IdTicket = request.IdTicket,
                    IdUsuarioAsignado = request.IdTecnico,
                    IdUsuarioAsignador = request.IdUsuarioAsignador,
                    TipoAsignacion = "Manual",
                    FechaAsignacion = DateTime.Now,
                    PuntajeAsignacion = null,
                    Justificacion = request.Justificacion ?? "Asignación manual realizada por administrador"
                };

                await _repository.AddAsync(asignacion);

                // 5. Registrar en historial
                var historial = new Historial_Tickets
                {
                    IdTicket = request.IdTicket,
                    IdUsuario = request.IdTecnico,
                    Accion = $"Asignación manual: {asignacion.Justificacion}",
                    FechaAccion = DateTime.Now
                };
                await _ticketRepository.AddHistorialAsync(historial);

                // 6. Enviar notificaciones (con manejo de errores)
                try
                {
                    var usuariosDestino = new List<int> { ticket.IdUsuarioSolicitante, request.IdTecnico };
                    await _notificacionService.CrearCambioEstadoTicketAsync(
                        usuariosDestino,
                        ticket.IdTicket,
                        "Pendiente",
                        "Asignado",
                        request.IdUsuarioAsignador.ToString(),
                        $"Asignación manual a {tecnico.Nombre}"
                    );
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Error al enviar notificación: {notifEx.Message}");
                }

                // 7. Recargar asignación con relaciones
                var asignacionCompleta = await _repository.FindByIdAsync(asignacion.IdAsignacion);
                var cargaTrabajo = await _repository.CountTicketsActivosByTecnicoAsync(request.IdTecnico);

                return new AsignacionResultDTO
                {
                    Exitoso = true,
                    Mensaje = "Asignación manual realizada exitosamente.",
                    Asignacion = asignacionCompleta != null ? _mapper.Map<AsignacionesDTO>(asignacionCompleta) : null,
                    Justificacion = asignacion.Justificacion,
                    TecnicoSeleccionado = new TecnicoSeleccionadoDTO
                    {
                        IdTecnico = tecnico.IdUsuario,
                        NombreTecnico = tecnico.Nombre,
                        CorreoTecnico = tecnico.Correo,
                        CargaActual = cargaTrabajo,
                        Disponible = true
                    }
                };
            }
            catch (Exception ex)
            {
                return new AsignacionResultDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error al asignar manualmente: {ex.Message} | Inner: {ex.InnerException?.Message}"
                };
            }
        }

        public async Task<IEnumerable<TecnicoDisponibleDTO>> GetTecnicosDisponiblesAsync(int? idTicket = null)
        {
            try
            {
                // Obtener técnicos usando ListByRolAsync que ya funciona correctamente
                var tecnicos = await _usuarioRepository.ListByRolAsync("Técnico");

                if (!tecnicos.Any())
                {
                    return new List<TecnicoDisponibleDTO>();
                }

                var tecnicosDisponibles = new List<TecnicoDisponibleDTO>();

                string categoriaRequerida = null;
                if (idTicket.HasValue)
                {
                    var ticket = await _ticketRepository.FindByIdAsync(idTicket.Value);
                    categoriaRequerida = ticket?.Categoria?.nombre_categoria;
                }

                foreach (var tecnico in tecnicos)
                {
                    try
                    {
                        var ticketsActivos = await _repository.CountTicketsActivosByTecnicoAsync(tecnico.IdUsuario);
                        var ticketsPendientes = await _repository.CountTicketsPendientesByTecnicoAsync(tecnico.IdUsuario);
                        var ticketsEnProceso = await _repository.CountTicketsEnProcesoByTecnicoAsync(tecnico.IdUsuario);

                        string nivelCarga = ticketsActivos switch
                        {
                            <= 2 => "Baja",
                            <= 5 => "Media",
                            _ => "Alta"
                        };

                        // Simplificamos especialidades por ahora
                        var especialidades = new List<string> { "General" };
                        bool tieneEspecialidad = true;

                        tecnicosDisponibles.Add(new TecnicoDisponibleDTO
                        {
                            IdTecnico = tecnico.IdUsuario,
                            NombreTecnico = tecnico.Nombre ?? "Sin nombre",
                            CorreoTecnico = tecnico.Correo ?? "",
                            TicketsActivos = ticketsActivos,
                            TicketsPendientes = ticketsPendientes,
                            TicketsEnProceso = ticketsEnProceso,
                            Especialidades = especialidades,
                            TieneEspecialidad = tieneEspecialidad,
                            NivelCarga = nivelCarga,
                            Disponible = ticketsActivos < 10
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar técnico {tecnico.IdUsuario}: {ex.Message}");
                        continue;
                    }
                }

                return tecnicosDisponibles.OrderBy(t => t.TicketsActivos).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetTecnicosDisponiblesAsync: {ex.Message}");
                return new List<TecnicoDisponibleDTO>();
            }
        }

        public async Task<IEnumerable<TicketPendienteAsignacionDTO>> GetTicketsPendientesAsync()
        {
            var ticketsPendientes = await _ticketRepository.ListByEstadoAsync("Pendiente");
            var ticketsDTO = new List<TicketPendienteAsignacionDTO>();

            foreach (var ticket in ticketsPendientes)
            {
                int? horasRestantes = null;
                if (ticket.FechaLimiteResolucion.HasValue)
                {
                    var diferencia = ticket.FechaLimiteResolucion.Value - DateTime.Now;
                    horasRestantes = diferencia.TotalHours > 0 ? (int)diferencia.TotalHours : 0;
                }

                var colorUrgencia = DeterminarColorUrgencia(horasRestantes, ticket.Estado);

                ticketsDTO.Add(new TicketPendienteAsignacionDTO
                {
                    IdTicket = ticket.IdTicket,
                    Titulo = ticket.Titulo,
                    Descripcion = ticket.Descripcion,
                    Categoria = ticket.Categoria?.nombre_categoria ?? "Sin categoría",
                    Prioridad = ticket.SLA?.prioridad ?? "Media",
                    Estado = ticket.Estado,
                    FechaLimiteResolucion = ticket.FechaLimiteResolucion,
                    HorasRestantes = horasRestantes,
                    TiempoResolucionHoras = ticket.SLA?.tiempo_resolucion_horas,
                    ColorUrgencia = colorUrgencia,
                    FechaCreacion = ticket.FechaCreacion
                });
            }

            return ticketsDTO.OrderBy(t => t.HorasRestantes ?? int.MaxValue);
        }

        #endregion

        #region Métodos Auxiliares

        private List<AsignacionPorSemanaDTO> OrganizarPorSemana(List<AsignacionesTickets> asignaciones)
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

        private TicketAsignadoDTO MapearTicketAsignado(AsignacionesTickets asignacion)
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
                return "success";

            if (!horasRestantes.HasValue)
                return "secondary";

            if (horasRestantes <= 6)
                return "danger";

            if (horasRestantes <= 24)
                return "warning";

            return "info";
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
                "Asignado" => 25,
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

        #endregion
    }
}