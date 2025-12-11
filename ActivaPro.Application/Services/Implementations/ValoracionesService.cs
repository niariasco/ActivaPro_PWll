using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class ValoracionesService : IValoracionesService
    {
        private readonly IRepoValoraciones _repository;
        private readonly IRepoTicketes _ticketRepository;
        private readonly INotificacionService _notificacionService;
        private readonly IMapper _mapper;

        public ValoracionesService(
            IRepoValoraciones repository,
            IRepoTicketes ticketRepository,
            INotificacionService notificacionService,
            IMapper mapper)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _notificacionService = notificacionService;
            _mapper = mapper;
        }

        // ========== CONSULTAS ==========

        public async Task<ValoracionDTO?> FindByIdAsync(int id)
        {
            var valoracion = await _repository.FindByIdAsync(id);
            if (valoracion == null)
                return null;

            return MapToDTO(valoracion);
        }

        public async Task<ValoracionDTO?> FindByTicketIdAsync(int idTicket)
        {
            var valoracion = await _repository.FindByTicketIdAsync(idTicket);
            if (valoracion == null)
                return null;

            return MapToDTO(valoracion);
        }

        public async Task<IEnumerable<ValoracionDTO>> ListAsync()
        {
            var valoraciones = await _repository.ListAsync();
            return valoraciones.Select(v => MapToDTO(v));
        }

        public async Task<IEnumerable<ValoracionDTO>> ListByRolAsync(int idUsuario, string rol)
        {
            ICollection<Valoracion_Notificaciones> valoraciones;

            switch (rol.ToLower())
            {
                case "administrador":
                case "coordinador":
                    valoraciones = await _repository.ListAsync();
                    break;

                case "técnico":
                case "tecnico":
                    valoraciones = await _repository.ListByTecnicoAsync(idUsuario);
                    break;

                case "cliente":
                    valoraciones = await _repository.ListByClienteAsync(idUsuario);
                    break;

                default:
                    valoraciones = new List<Valoracion_Notificaciones>();
                    break;
            }

            return valoraciones.Select(v => MapToDTO(v));
        }

        // ========== PREPARACIÓN DE DTOs ==========

        public async Task<ValoracionCreateDTO> PrepareCreateDTOAsync(int idTicket, int idCliente)
        {
            var ticket = await _ticketRepository.FindByIdAsync(idTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {idTicket} no encontrado");

            return new ValoracionCreateDTO
            {
                IdTicket = ticket.IdTicket,
                TituloTicket = ticket.Titulo,
                EstadoTicket = ticket.Estado,
                FechaCreacionTicket = ticket.FechaCreacion,
                IdUsuarioSolicitante = ticket.IdUsuarioSolicitante
            };
        }

        // ========== VALIDACIONES ==========

        public async Task<(bool esValido, string mensaje)> ValidarCreacionValoracionAsync(int idTicket, int idCliente)
        {
            // 1. Verificar que el ticket existe
            var ticket = await _ticketRepository.FindByIdAsync(idTicket);
            if (ticket == null)
                return (false, "El ticket no existe");

            // 2. Verificar que el cliente es el solicitante del ticket
            if (ticket.IdUsuarioSolicitante != idCliente)
                return (false, "Solo el cliente que creó el ticket puede valorarlo");

            // 3. Verificar que el ticket está cerrado
            if (ticket.Estado != "Cerrado")
                return (false, "Solo se pueden valorar tickets cerrados");

            // 4. Verificar que no existe una valoración previa
            var existeValoracion = await _repository.ExisteValoracionParaTicketAsync(idTicket);
            if (existeValoracion)
                return (false, "Este ticket ya ha sido valorado. No se permiten valoraciones duplicadas.");

            return (true, string.Empty);
        }

        // ========== CREACIÓN CON REGISTRO EN HISTORIAL ==========

        public async Task<int> CreateValoracionAsync(ValoracionCreateDTO dto, int idCliente)
        {
            // ✅ VALIDACIONES
            var (esValido, mensaje) = await ValidarCreacionValoracionAsync(dto.IdTicket, idCliente);
            if (!esValido)
                throw new InvalidOperationException(mensaje);

            // ✅ BUSCAR O CREAR NOTIFICACIÓN PARA EL TICKET
            var notificacion = await _notificacionService.ObtenerOCrearNotificacionParaTicketAsync(dto.IdTicket, idCliente);

            // ✅ CREAR LA VALORACIÓN
            var valoracion = new Valoracion_Notificaciones
            {
                IdNotificacion = notificacion.IdNotificacion,
                IdUsuario = idCliente,
                Puntaje = dto.Puntaje,
                Comentario = dto.Comentario,
                FechaValoracion = DateTime.Now
            };

            await _repository.CreateAsync(valoracion);

            // ✅ OBTENER EL TICKET
            var ticket = await _ticketRepository.FindByIdAsync(dto.IdTicket);
            if (ticket != null)
            {
                // ⭐ NIVEL DE SATISFACCIÓN
                string nivelSatisfaccion = dto.Puntaje switch
                {
                    5 => "Excelente",
                    4 => "Muy Bueno",
                    3 => "Bueno",
                    2 => "Regular",
                    1 => "Malo",
                    _ => "Sin calificación"
                };

                // ⭐ REGISTRAR EN HISTORIAL DEL TICKET
                var historial = new Historial_Tickets
                {
                    IdTicket = ticket.IdTicket,
                    IdUsuario = idCliente,
                    Accion = $"⭐ Valoración registrada: {nivelSatisfaccion} ({dto.Puntaje}/5 estrellas)",
                    EstadoAnterior = null,
                    EstadoNuevo = null,
                    Comentario = dto.Comentario.Length > 200
                        ? dto.Comentario.Substring(0, 197) + "..."
                        : dto.Comentario,
                    FechaAccion = DateTime.Now
                };

                await _ticketRepository.AddHistorialAsync(historial);

                // ✅ NOTIFICAR AL TÉCNICO ASIGNADO
                if (ticket.IdUsuarioAsignado.HasValue)
                {
                    await _notificacionService.CrearEventoTicketAsync(
                        new[] { ticket.IdUsuarioAsignado.Value },
                        ticket.IdTicket,
                        "Valoración",
                        $"Ticket #{ticket.IdTicket} valorado: {nivelSatisfaccion} ({dto.Puntaje}/5 ⭐)",
                        ticket.UsuarioSolicitante?.Nombre ?? "Cliente");
                }
            }

            return valoracion.IdValoracion;
        }

        // ========== ESTADÍSTICAS ==========

        public async Task<ValoracionEstadisticasDTO> GetEstadisticasAsync()
        {
            var distribucion = await _repository.GetDistribucionPuntajesAsync();
            var total = await _repository.CountAsync();
            var promedio = await _repository.GetPromedioGeneralAsync();

            return new ValoracionEstadisticasDTO
            {
                TotalValoraciones = total,
                PromedioGeneral = Math.Round(promedio, 2),
                Excelente = distribucion.ContainsKey(5) ? distribucion[5] : 0,
                MuyBueno = distribucion.ContainsKey(4) ? distribucion[4] : 0,
                Bueno = distribucion.ContainsKey(3) ? distribucion[3] : 0,
                Regular = distribucion.ContainsKey(2) ? distribucion[2] : 0,
                Malo = distribucion.ContainsKey(1) ? distribucion[1] : 0
            };
        }

        public async Task<ValoracionEstadisticasDTO> GetEstadisticasByTecnicoAsync(int idTecnico)
        {
            var valoraciones = await _repository.ListByTecnicoAsync(idTecnico);

            if (!valoraciones.Any())
            {
                return new ValoracionEstadisticasDTO
                {
                    TotalValoraciones = 0,
                    PromedioGeneral = 0
                };
            }

            var total = valoraciones.Count;
            var promedio = valoraciones.Average(v => (double)v.Puntaje);

            return new ValoracionEstadisticasDTO
            {
                TotalValoraciones = total,
                PromedioGeneral = Math.Round(promedio, 2),
                Excelente = valoraciones.Count(v => v.Puntaje == 5),
                MuyBueno = valoraciones.Count(v => v.Puntaje == 4),
                Bueno = valoraciones.Count(v => v.Puntaje == 3),
                Regular = valoraciones.Count(v => v.Puntaje == 2),
                Malo = valoraciones.Count(v => v.Puntaje == 1)
            };
        }

        // ========== MAPEO PRIVADO ==========

        private ValoracionDTO MapToDTO(Valoracion_Notificaciones valoracion)
        {
            // Obtener el ticket a través de la notificación
            var notificacion = valoracion.IdNotificacionNavigation;
            Tickets? ticket = null;

            if (notificacion?.IdTicket != null)
            {
                // Buscar el ticket por su ID
                ticket = _ticketRepository.FindByIdAsync(notificacion.IdTicket.Value).Result;
            }

            return new ValoracionDTO
            {
                IdValoracion = valoracion.IdValoracion,
                IdNotificacion = valoracion.IdNotificacion,
                IdUsuario = valoracion.IdUsuario,
                IdTicket = ticket?.IdTicket ?? 0,
                Puntaje = valoracion.Puntaje,
                Comentario = valoracion.Comentario ?? string.Empty,
                FechaValoracion = valoracion.FechaValoracion,

                // Información del ticket
                TituloTicket = ticket?.Titulo ?? "Sin título",
                EstadoTicket = ticket?.Estado ?? "Desconocido",
                CategoriaNombre = ticket?.Categoria?.nombre_categoria ?? "Sin categoría",
                FechaCreacionTicket = ticket?.FechaCreacion ?? DateTime.MinValue,
                FechaResolucionTicket = ticket?.FechaActualizacion,

                // Información del cliente
                NombreCliente = ticket?.UsuarioSolicitante?.Nombre ?? valoracion.IdUsuarioNavigation?.Nombre ?? "Desconocido",

                // Información del técnico
                NombreTecnico = ticket?.UsuarioAsignado?.Nombre ?? "Sin asignar"
            };
        }
    }
}