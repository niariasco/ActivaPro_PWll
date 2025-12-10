using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class AsignacionesService : IAsignacionesService
    {
        private readonly IRepoAsignaciones _repoAsignaciones;
        private readonly IRepoTicketes _repoTicketes;
        private readonly IRepoUsuarios _repoUsuarios;
        private readonly ActivaProContext _context;
        private readonly IMapper _mapper;

        // Constantes para el algoritmo de autotriage
        private const int PESO_PRIORIDAD = 1000;
        private const int PESO_CARGA_TRABAJO = 50;
        private const int BONUS_ESPECIALIDAD = 100;
        private const int LIMITE_CARGA_TECNICO = 10; // Máximo de tickets activos

        public AsignacionesService(
            IRepoAsignaciones repoAsignaciones,
            IRepoTicketes repoTicketes,
            IRepoUsuarios repoUsuarios,
            ActivaProContext context,
            IMapper mapper)
        {
            _repoAsignaciones = repoAsignaciones;
            _repoTicketes = repoTicketes;
            _repoUsuarios = repoUsuarios;
            _context = context;
            _mapper = mapper;
        }

        // ========== ASIGNACIÓN AUTOMÁTICA (AUTOTRIAGE) ==========

        /// <summary>
        /// Asigna un ticket automáticamente usando el algoritmo de autotriage
        /// Fórmula: Puntaje = (Prioridad × 1000) - TiempoRestanteSLA - (CargaTrabajo × 50) + BonusEspecialidad
        /// </summary>
        public async Task<AsignacionResultDTO> AsignarAutomaticamenteAsync(int idTicket)
        {
            try
            {
                // 1. Obtener el ticket con todas sus relaciones
                var ticket = await _repoTicketes.FindByIdAsync(idTicket);
                if (ticket == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El ticket no existe"
                    };
                }

                // 2. Validar que el ticket esté pendiente
                if (ticket.Estado != "Pendiente")
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = $"El ticket está en estado '{ticket.Estado}' y no puede ser asignado automáticamente"
                    };
                }

                // 3. Obtener técnicos disponibles
                var tecnicos = await ObtenerTecnicosParaAsignacionAsync(idTicket);
                if (!tecnicos.Any())
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "No hay técnicos disponibles para asignar este ticket"
                    };
                }

                // 4. Calcular puntaje base del ticket
                var puntajeBaseTicket = CalcularPuntajeTicket(ticket);

                // 5. Calcular puntajes de cada técnico para este ticket
                var puntajes = new List<PuntajeAsignacionDTO>();
                foreach (var tecnico in tecnicos)
                {
                    var puntaje = await CalcularPuntajeTecnicoAsync(ticket, tecnico, puntajeBaseTicket);
                    puntajes.Add(puntaje);
                }

                // 6. Seleccionar el técnico con mayor puntaje
                var mejorTecnico = puntajes.OrderByDescending(p => p.Puntaje).First();

                // 7. Crear la asignación
                var asignacion = new AsignacionesTickets
                {
                    IdTicket = idTicket,
                    IdUsuarioAsignado = mejorTecnico.IdTecnico,
                    TipoAsignacion = "Automatica",
                    FechaAsignacion = DateTime.Now,
                    PuntajeAsignacion = mejorTecnico.Puntaje,
                    Justificacion = mejorTecnico.Justificacion
                };

                // 8. Guardar la asignación
                await _repoAsignaciones.AddAsync(asignacion);

                // 9. Actualizar el estado del ticket
                ticket.Estado = "Asignado";
                ticket.IdUsuarioAsignado = mejorTecnico.IdTecnico;
                ticket.FechaActualizacion = DateTime.Now;
                await _repoTicketes.UpdateAsync(ticket);

                // 10. Retornar resultado exitoso
                return new AsignacionResultDTO
                {
                    Exitoso = true,
                    Mensaje = $"Ticket #{idTicket} asignado exitosamente mediante Autotriage",
                    Puntaje = mejorTecnico.Puntaje,
                    Justificacion = mejorTecnico.Justificacion,
                    TecnicoSeleccionado = new TecnicoSeleccionadoDTO
                    {
                        IdTecnico = mejorTecnico.IdTecnico,
                        NombreTecnico = mejorTecnico.NombreTecnico,
                        CorreoTecnico = tecnicos.First(t => t.IdTecnico == mejorTecnico.IdTecnico).CorreoTecnico,
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
                    Mensaje = $"Error al asignar ticket automáticamente: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Calcula el puntaje base del ticket según su prioridad y tiempo restante de SLA
        /// Fórmula: Puntaje = (Prioridad × 1000) - TiempoRestanteSLA
        /// Mayor puntaje = Mayor urgencia
        /// </summary>
        private int CalcularPuntajeTicket(Tickets ticket)
        {
            // Obtener valor de prioridad desde SLA
            int valorPrioridad = 2; // Por defecto Media

            if (ticket.SLA != null && !string.IsNullOrEmpty(ticket.SLA.prioridad))
            {
                valorPrioridad = ticket.SLA.prioridad switch
                {
                    "Crítica" => 4,
                    "Alta" => 3,
                    "Media" => 2,
                    "Baja" => 1,
                    _ => 2
                };
            }

            int puntajePrioridad = valorPrioridad * PESO_PRIORIDAD;

            // Calcular tiempo restante del SLA en horas
            int tiempoRestanteHoras = 999; // Valor por defecto alto si no hay SLA

            if (ticket.FechaLimiteResolucion.HasValue)
            {
                var tiempoRestante = ticket.FechaLimiteResolucion.Value - DateTime.Now;
                tiempoRestanteHoras = Math.Max(0, (int)tiempoRestante.TotalHours);
            }

            // Fórmula principal: Mayor puntaje = Mayor urgencia
            // Se resta el tiempo porque menos tiempo = más urgente
            int puntajeTotal = puntajePrioridad - tiempoRestanteHoras;

            return puntajeTotal;
        }

        /// <summary>
        /// Calcula el puntaje de un técnico específico para asignarle un ticket
        /// Considera: puntaje base del ticket, carga de trabajo y especialidad
        /// </summary>
        private async Task<PuntajeAsignacionDTO> CalcularPuntajeTecnicoAsync(
            Tickets ticket,
            TecnicoDisponibleDTO tecnico,
            int puntajeBaseTicket)
        {
            // Iniciar con el puntaje base del ticket
            decimal puntajeFinal = puntajeBaseTicket;

            var justificaciones = new List<string>();

            // CRITERIO 1: Puntaje base del ticket (prioridad - tiempo SLA)
            justificaciones.Add($"Puntaje base del ticket: {puntajeBaseTicket} pts");

            // CRITERIO 2: Ajustar por carga de trabajo (menos carga = mejor)
            int penalizacionCarga = tecnico.TicketsActivos * PESO_CARGA_TRABAJO;
            puntajeFinal -= penalizacionCarga;
            justificaciones.Add($"Carga actual: {tecnico.TicketsActivos} tickets (-{penalizacionCarga} pts)");

            // CRITERIO 3: Bonus por especialidad
            if (tecnico.TieneEspecialidad)
            {
                puntajeFinal += BONUS_ESPECIALIDAD;
                justificaciones.Add($"Especializado en '{ticket.Categoria?.nombre_categoria}' (+{BONUS_ESPECIALIDAD} pts)");
            }
            else
            {
                justificaciones.Add($"Sin especialidad en '{ticket.Categoria?.nombre_categoria}' (+0 pts)");
            }

            // Crear justificación completa y estructurada
            var justificacionCompleta = GenerarJustificacionDetallada(
                ticket,
                tecnico,
                puntajeBaseTicket,
                justificaciones,
                puntajeFinal
            );

            return new PuntajeAsignacionDTO
            {
                IdTecnico = tecnico.IdTecnico,
                NombreTecnico = tecnico.NombreTecnico,
                Puntaje = puntajeFinal,
                ValorPrioridad = puntajeBaseTicket,
                CargaTrabajo = tecnico.TicketsActivos,
                TieneEspecialidad = tecnico.TieneEspecialidad,
                Justificacion = justificacionCompleta
            };
        }

        /// <summary>
        /// Genera una justificación detallada y legible de la asignación
        /// </summary>
        private string GenerarJustificacionDetallada(
            Tickets ticket,
            TecnicoDisponibleDTO tecnico,
            int puntajeBase,
            List<string> justificaciones,
            decimal puntajeFinal)
        {
            var sb = new StringBuilder();

            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║       CÁLCULO DE ASIGNACIÓN AUTOMÁTICA (AUTOTRIAGE)         ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine("📋 INFORMACIÓN DEL TICKET:");
            sb.AppendLine($"   • ID: #{ticket.IdTicket}");
            sb.AppendLine($"   • Título: {ticket.Titulo}");
            sb.AppendLine($"   • Prioridad: {ticket.SLA?.prioridad ?? "Sin SLA"}");
            sb.AppendLine($"   • Categoría: {ticket.Categoria?.nombre_categoria ?? "Sin categoría"}");
            if (ticket.FechaLimiteResolucion.HasValue)
            {
                var horasRestantes = (ticket.FechaLimiteResolucion.Value - DateTime.Now).TotalHours;
                sb.AppendLine($"   • Tiempo restante SLA: {Math.Max(0, (int)horasRestantes)} horas");
            }
            sb.AppendLine();

            sb.AppendLine("👤 TÉCNICO ASIGNADO:");
            sb.AppendLine($"   • Nombre: {tecnico.NombreTecnico}");
            sb.AppendLine($"   • Carga actual: {tecnico.TicketsActivos} tickets activos");
            sb.AppendLine($"   • Nivel de carga: {tecnico.NivelCarga}");
            sb.AppendLine($"   • Especialidad: {(tecnico.TieneEspecialidad ? "✓ Sí" : "✗ No")}");
            sb.AppendLine();

            sb.AppendLine("🔢 DESGLOSE DEL CÁLCULO:");
            sb.AppendLine("   Fórmula: Puntaje = (Prioridad × 1000) - TiempoRestante_SLA - (Carga × 50) + Especialidad");
            sb.AppendLine();
            foreach (var justificacion in justificaciones)
            {
                sb.AppendLine($"   • {justificacion}");
            }
            sb.AppendLine();
            sb.AppendLine("   ═══════════════════════════════════════════════════════════");
            sb.AppendLine($"   ✨ PUNTAJE FINAL: {puntajeFinal:F2} puntos");
            sb.AppendLine("   ═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine("📊 CRITERIOS DE SELECCIÓN:");
            sb.AppendLine("   ✓ Mayor puntaje entre todos los técnicos disponibles");
            sb.AppendLine("   ✓ Dentro del límite de carga permitido (< 10 tickets)");
            sb.AppendLine("   ✓ Estado activo y disponible para asignaciones");
            sb.AppendLine();

            sb.AppendLine($"⏰ Fecha de asignación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"🤖 Tipo de asignación: Automática (Autotriage)");

            return sb.ToString();
        }

        /// <summary>
        /// Obtiene los técnicos disponibles con su información de carga
        /// </summary>
        private async Task<List<TecnicoDisponibleDTO>> ObtenerTecnicosParaAsignacionAsync(int? idTicket = null)
        {
            // Obtener ticket si se proporciona ID
            Tickets ticket = null;
            if (idTicket.HasValue)
            {
                ticket = await _repoTicketes.FindByIdAsync(idTicket.Value);
            }

            // Obtener todos los técnicos activos usando el contexto
            var tecnicos = await _context.Tecnico
                .Include(t => t.Usuario)
                    .ThenInclude(u => u.UsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                .Where(t => t.Disponible && t.Usuario != null)
                .ToListAsync();

            var tecnicosDisponibles = new List<TecnicoDisponibleDTO>();

            foreach (var tecnico in tecnicos)
            {
                // Contar tickets activos del técnico
                int ticketsActivos = await _context.Ticketes
                    .Where(t => t.IdUsuarioAsignado == tecnico.IdUsuario
                        && (t.Estado == "Asignado" || t.Estado == "En Proceso"))
                    .CountAsync();

                int ticketsPendientes = await _context.Ticketes
                    .Where(t => t.IdUsuarioAsignado == tecnico.IdUsuario && t.Estado == "Asignado")
                    .CountAsync();

                int ticketsEnProceso = await _context.Ticketes
                    .Where(t => t.IdUsuarioAsignado == tecnico.IdUsuario && t.Estado == "En Proceso")
                    .CountAsync();

                // Verificar especialidad si hay ticket
                bool tieneEspecialidad = false;
                var especialidades = new List<string>();

                if (ticket?.IdCategoria != null)
                {
                    // Obtener especialidades del técnico usando Tecnico_Especialidad
                    var especialidadesTecnico = await _context.Set<Tecnico_Especialidad>()
                        .Include(te => te.EspecialidadU)
                        .Where(te => te.IdTecnico == tecnico.IdTecnico)
                        .ToListAsync();

                    // Obtener especialidades de la categoría usando Categoria_Especialidad
                    var especialidadesCategoria = await _context.Set<Categoria_Especialidad>()
                        .Include(ce => ce.Especialidad)
                        .Where(ce => ce.id_categoria == ticket.IdCategoria)
                        .Select(ce => ce.Especialidad.NombreEspecialidad)
                        .ToListAsync();

                    especialidades = especialidadesTecnico
                        .Select(e => e.EspecialidadU?.NombreEspecialidadU ?? "")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();

                    // Verificar coincidencia entre especialidades del técnico y de la categoría
                    tieneEspecialidad = especialidadesTecnico
                        .Any(te => especialidadesCategoria.Contains(te.EspecialidadU?.NombreEspecialidadU ?? ""));
                }

                // Determinar nivel de carga
                string nivelCarga = ticketsActivos <= 3 ? "Baja" :
                                  ticketsActivos <= 7 ? "Media" : "Alta";

                // Validar disponibilidad (no sobrepasar límite)
                bool disponible = ticketsActivos < LIMITE_CARGA_TECNICO;

                tecnicosDisponibles.Add(new TecnicoDisponibleDTO
                {
                    IdTecnico = tecnico.IdUsuario,
                    NombreTecnico = tecnico.Usuario?.Nombre ?? "Sin nombre",
                    CorreoTecnico = tecnico.Usuario?.Correo ?? "Sin correo",
                    TicketsActivos = ticketsActivos,
                    TicketsPendientes = ticketsPendientes,
                    TicketsEnProceso = ticketsEnProceso,
                    Especialidades = especialidades,
                    TieneEspecialidad = tieneEspecialidad,
                    NivelCarga = nivelCarga,
                    Disponible = disponible
                });
            }

            // Retornar solo técnicos disponibles
            return tecnicosDisponibles.Where(t => t.Disponible).ToList();
        }

        /// <summary>
        /// Asigna todos los tickets pendientes automáticamente
        /// </summary>
        public async Task<List<AsignacionResultDTO>> AsignarTodosPendientesAsync()
        {
            var resultados = new List<AsignacionResultDTO>();
            var ticketsPendientes = await _repoTicketes.ListByEstadoAsync("Pendiente");

            foreach (var ticket in ticketsPendientes)
            {
                var resultado = await AsignarAutomaticamenteAsync(ticket.IdTicket);
                resultados.Add(resultado);

                // Pequeña pausa para evitar sobrecarga
                await Task.Delay(100);
            }

            return resultados;
        }

        // ========== ASIGNACIÓN MANUAL ==========

        /// <summary>
        /// Asigna un ticket manualmente a un técnico específico
        /// </summary>
        public async Task<AsignacionResultDTO> AsignarManualmenteAsync(AsignacionManualRequestDTO request)
        {
            try
            {
                // 1. Validar ticket
                var ticket = await _repoTicketes.FindByIdAsync(request.IdTicket);
                if (ticket == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El ticket no existe"
                    };
                }

                if (ticket.Estado != "Pendiente")
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = $"El ticket está en estado '{ticket.Estado}' y no puede ser asignado"
                    };
                }

                // 2. Validar técnico
                var tecnico = await _context.Tecnico
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.IdUsuario == request.IdTecnico);

                if (tecnico == null || tecnico.Usuario == null)
                {
                    return new AsignacionResultDTO
                    {
                        Exitoso = false,
                        Mensaje = "El técnico seleccionado no existe o no es válido"
                    };
                }

                // 3. Crear justificación
                var justificacion = new StringBuilder();
                justificacion.AppendLine("╔══════════════════════════════════════════════════════════════╗");
                justificacion.AppendLine("║           ASIGNACIÓN MANUAL DE TICKET                        ║");
                justificacion.AppendLine("╚══════════════════════════════════════════════════════════════╝");
                justificacion.AppendLine();
                justificacion.AppendLine($"📋 Ticket: #{ticket.IdTicket} - {ticket.Titulo}");
                justificacion.AppendLine($"👤 Técnico asignado: {tecnico.Usuario.Nombre}");
                justificacion.AppendLine($"👨‍💼 Asignado por: Usuario ID {request.IdUsuarioAsignador}");
                justificacion.AppendLine($"⏰ Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                justificacion.AppendLine();
                justificacion.AppendLine("💬 Justificación:");
                justificacion.AppendLine($"   {request.Justificacion ?? "Sin justificación específica"}");

                // 4. Crear la asignación
                var asignacion = new AsignacionesTickets
                {
                    IdTicket = request.IdTicket,
                    IdUsuarioAsignado = request.IdTecnico,
                    IdUsuarioAsignador = request.IdUsuarioAsignador,
                    TipoAsignacion = "Manual",
                    FechaAsignacion = DateTime.Now,
                    Justificacion = justificacion.ToString()
                };

                // 5. Guardar la asignación
                await _repoAsignaciones.AddAsync(asignacion);

                // 6. Actualizar el ticket
                ticket.Estado = "Asignado";
                ticket.IdUsuarioAsignado = request.IdTecnico;
                ticket.FechaActualizacion = DateTime.Now;
                await _repoTicketes.UpdateAsync(ticket);

                // 7. Obtener carga actual
                int cargaActual = await _context.Ticketes
                    .Where(t => t.IdUsuarioAsignado == request.IdTecnico
                        && (t.Estado == "Asignado" || t.Estado == "En Proceso"))
                    .CountAsync();

                // 8. Retornar resultado
                return new AsignacionResultDTO
                {
                    Exitoso = true,
                    Mensaje = $"Ticket #{request.IdTicket} asignado manualmente",
                    Justificacion = justificacion.ToString(),
                    TecnicoSeleccionado = new TecnicoSeleccionadoDTO
                    {
                        IdTecnico = tecnico.IdUsuario,
                        NombreTecnico = tecnico.Usuario.Nombre,
                        CorreoTecnico = tecnico.Usuario.Correo,
                        CargaActual = cargaActual,
                        Disponible = true
                    }
                };
            }
            catch (Exception ex)
            {
                return new AsignacionResultDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error al asignar ticket manualmente: {ex.Message}"
                };
            }
        }

        // ========== CONSULTAS ==========

        public async Task<IEnumerable<TicketPendienteAsignacionDTO>> GetTicketsPendientesAsync()
        {
            var ticketsPendientes = await _repoTicketes.ListByEstadoAsync("Pendiente");

            return ticketsPendientes.Select(t => new TicketPendienteAsignacionDTO
            {
                IdTicket = t.IdTicket,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion ?? "Sin descripción",
                Categoria = t.Categoria?.nombre_categoria ?? "Sin categoría",
                Prioridad = t.SLA?.prioridad ?? "Media",
                Estado = t.Estado,
                FechaLimiteResolucion = t.FechaLimiteResolucion,
                HorasRestantes = t.FechaLimiteResolucion.HasValue
                    ? Math.Max(0, (int)(t.FechaLimiteResolucion.Value - DateTime.Now).TotalHours)
                    : null,
                TiempoResolucionHoras = t.SLA?.tiempo_resolucion_horas,
                ColorUrgencia = DeterminarColorUrgencia(t),
                FechaCreacion = t.FechaCreacion
            }).ToList();
        }

        public async Task<IEnumerable<TecnicoDisponibleDTO>> GetTecnicosDisponiblesAsync(int? idTicket = null)
        {
            return await ObtenerTecnicosParaAsignacionAsync(idTicket);
        }

        private string DeterminarColorUrgencia(Tickets ticket)
        {
            if (!ticket.FechaLimiteResolucion.HasValue)
                return "secondary";

            var horasRestantes = (ticket.FechaLimiteResolucion.Value - DateTime.Now).TotalHours;

            if (horasRestantes <= 6) return "danger";
            if (horasRestantes <= 24) return "warning";
            return "success";
        }

        // Implementar métodos restantes de la interfaz según sea necesario...
        public async Task<IEnumerable<TecnicoAsignacionesDTO>> GetAsignacionesPorTecnicoAsync()
        {
            try
            {
                // 1. Obtener todos los técnicos activos
                var tecnicos = await _context.Tecnico
                    .Include(t => t.Usuario)
                    .Where(t => t.Disponible && t.Usuario != null)
                    .ToListAsync();

                var tecnicosDTOs = new List<TecnicoAsignacionesDTO>();

                foreach (var tecnico in tecnicos)
                {
                    // 2. Obtener asignaciones del técnico
                    var asignaciones = await _repoAsignaciones.ListByTecnicoAsync(tecnico.IdUsuario);

                    // 3. Calcular estadísticas
                    var ticketsAsignados = asignaciones.Select(a => a.IdTicketNavigation).ToList();

                    int totalTickets = ticketsAsignados.Count;
                    int ticketsPendientes = ticketsAsignados.Count(t => t.Estado == "Asignado");
                    int ticketsEnProceso = ticketsAsignados.Count(t => t.Estado == "En Proceso");
                    int ticketsCerrados = ticketsAsignados.Count(t => t.Estado == "Cerrado");

                    // 4. Agrupar asignaciones por semana
                    var asignacionesPorSemana = asignaciones
                        .Where(a => a.FechaAsignacion.HasValue)
                        .GroupBy(a => new
                        {
                            Semana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                a.FechaAsignacion.Value,
                                CalendarWeekRule.FirstDay,
                                DayOfWeek.Monday),
                            Anio = a.FechaAsignacion.Value.Year
                        })
                        .OrderByDescending(g => g.Key.Anio)
                        .ThenByDescending(g => g.Key.Semana)
                        .Select(g => new AsignacionPorSemanaDTO
                        {
                            NumeroSemana = g.Key.Semana,
                            Anio = g.Key.Anio,
                            RangoFechas = CalcularRangoFechasSemana(g.Key.Semana, g.Key.Anio),
                            Tickets = g.Select(a => MapearTicketAsignado(a.IdTicketNavigation, a.FechaAsignacion.Value)).ToList()
                        })
                        .ToList();

                    // 5. Crear DTO del técnico
                    tecnicosDTOs.Add(new TecnicoAsignacionesDTO
                    {
                        IdTecnico = tecnico.IdUsuario,
                        NombreTecnico = tecnico.Usuario.Nombre,
                        CorreoTecnico = tecnico.Usuario.Correo,
                        TotalTicketsAsignados = totalTickets,
                        TicketsPendientes = ticketsPendientes,
                        TicketsEnProceso = ticketsEnProceso,
                        TicketsCerrados = ticketsCerrados,
                        AsignacionesPorSemana = asignacionesPorSemana
                    });
                }

                return tecnicosDTOs.OrderBy(t => t.NombreTecnico);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener asignaciones por técnico: {ex.Message}", ex);
            }
        }

        public async Task<TecnicoAsignacionesDTO> GetAsignacionesByTecnicoIdAsync(int idTecnico)
        {
            try
            {
                // 1. Obtener información del técnico
                var tecnico = await _context.Tecnico
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.IdUsuario == idTecnico);

                if (tecnico == null || tecnico.Usuario == null)
                {
                    throw new Exception($"No se encontró el técnico con ID {idTecnico}");
                }

                // 2. Obtener asignaciones del técnico
                var asignaciones = await _repoAsignaciones.ListByTecnicoAsync(idTecnico);

                // 3. Calcular estadísticas
                var ticketsAsignados = asignaciones.Select(a => a.IdTicketNavigation).ToList();

                int totalTickets = ticketsAsignados.Count;
                int ticketsPendientes = ticketsAsignados.Count(t => t.Estado == "Asignado");
                int ticketsEnProceso = ticketsAsignados.Count(t => t.Estado == "En Proceso");
                int ticketsCerrados = ticketsAsignados.Count(t => t.Estado == "Cerrado");

                // 4. Agrupar asignaciones por semana
                var asignacionesPorSemana = asignaciones
                    .Where(a => a.FechaAsignacion.HasValue)
                    .GroupBy(a => new
                    {
                        Semana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            a.FechaAsignacion.Value,
                            CalendarWeekRule.FirstDay,
                            DayOfWeek.Monday),
                        Anio = a.FechaAsignacion.Value.Year
                    })
                    .OrderByDescending(g => g.Key.Anio)
                    .ThenByDescending(g => g.Key.Semana)
                    .Select(g => new AsignacionPorSemanaDTO
                    {
                        NumeroSemana = g.Key.Semana,
                        Anio = g.Key.Anio,
                        RangoFechas = CalcularRangoFechasSemana(g.Key.Semana, g.Key.Anio),
                        Tickets = g.Select(a => MapearTicketAsignado(a.IdTicketNavigation, a.FechaAsignacion.Value)).ToList()
                    })
                    .ToList();

                // 5. Crear DTO del técnico
                return new TecnicoAsignacionesDTO
                {
                    IdTecnico = idTecnico,
                    NombreTecnico = tecnico.Usuario.Nombre,
                    CorreoTecnico = tecnico.Usuario.Correo,
                    TotalTicketsAsignados = totalTickets,
                    TicketsPendientes = ticketsPendientes,
                    TicketsEnProceso = ticketsEnProceso,
                    TicketsCerrados = ticketsCerrados,
                    AsignacionesPorSemana = asignacionesPorSemana
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener asignaciones del técnico {idTecnico}: {ex.Message}", ex);
            }
        }

        // Método privado para calcular el rango de fechas de una semana específica de un año
        private string CalcularRangoFechasSemana(int numeroSemana, int anio)
        {
            // Obtener el primer día del año
            DateTime primerDiaAnio = new DateTime(anio, 1, 1);

            // Calcular el número de días hasta el primer día de la semana (lunes)
            int diasOffset = DayOfWeek.Monday - primerDiaAnio.DayOfWeek;
            if (diasOffset < 0) diasOffset += 7;

            // Obtener el primer lunes del año
            DateTime primerLunes = primerDiaAnio.AddDays(diasOffset);

            // Calcular la fecha de inicio de la semana deseada
            DateTime fechaInicioSemana = primerLunes.AddDays((numeroSemana - 1) * 7);

            // La fecha de fin es 6 días después
            DateTime fechaFinSemana = fechaInicioSemana.AddDays(6);

            // Ajustar si la semana cae fuera del año
            if (fechaInicioSemana.Year < anio)
                fechaInicioSemana = new DateTime(anio, 1, 1);
            if (fechaFinSemana.Year > anio)
                fechaFinSemana = new DateTime(anio, 12, 31);

            return $"{fechaInicioSemana:dd/MM/yyyy} - {fechaFinSemana:dd/MM/yyyy}";
        }

        private TicketAsignadoDTO MapearTicketAsignado(Tickets ticket, DateTime fechaAsignacion)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            return new TicketAsignadoDTO
            {
                IdTicket = ticket.IdTicket,
                Titulo = ticket.Titulo,
                Categoria = ticket.Categoria?.nombre_categoria ?? "Sin categoría",
                Estado = ticket.Estado,
                Prioridad = ticket.SLA?.prioridad ?? "Media",
                FechaLimiteResolucion = ticket.FechaLimiteResolucion,
                HorasRestantes = ticket.FechaLimiteResolucion.HasValue
                    ? Math.Max(0, (int)(ticket.FechaLimiteResolucion.Value - DateTime.Now).TotalHours)
                    : null,
                ColorUrgencia = DeterminarColorUrgencia(ticket),
                IconoCategoria = "", // Asignar el icono si aplica, o dejar vacío
                PorcentajeProgreso = 0, // Asignar el progreso si aplica, o dejar en 0
                FechaAsignacion = fechaAsignacion
            };
        }
    }
}