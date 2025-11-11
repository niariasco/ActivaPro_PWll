using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class TicketesService : ITicketesService
    {
        private readonly IRepoTicketes _repository;
        private readonly IRepoUsuarios _usuarioRepository;
        private readonly IRepoEtiquetas _etiquetaRepository;
        private readonly IRepoCategorias _categoriaRepository;
        private readonly IMapper _mapper;

        public TicketesService(
            IRepoTicketes repository,
            IRepoUsuarios usuarioRepository,
            IRepoEtiquetas etiquetaRepository,
            IRepoCategorias categoriaRepository,
            IMapper mapper)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _etiquetaRepository = etiquetaRepository;
            _categoriaRepository = categoriaRepository;
            _mapper = mapper;
        }

        public async Task<TicketesDTO?> FindByIdAsync(int id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket == null)
                return null;

            return _mapper.Map<TicketesDTO>(ticket);
        }

        public async Task<IEnumerable<TicketesDTO>> ListAsync()
        {
            var tickets = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<TicketesDTO>>(tickets);
        }

        public async Task<IEnumerable<TicketesDTO>> ListByRolAsync(int idUsuario, string rol)
        {
            ICollection<Tickets> tickets;

            switch (rol.ToLower())
            {
                case "administrador":
                    tickets = await _repository.ListAsync();
                    break;
                case "técnico":
                case "tecnico":
                    tickets = await _repository.ListByUsuarioAsignadoAsync(idUsuario);
                    break;
                case "cliente":
                    tickets = await _repository.ListByUsuarioSolicitanteAsync(idUsuario);
                    break;
                default:
                    // Por defecto, mostrar solo tickets del usuario
                    tickets = await _repository.ListByUsuarioSolicitanteAsync(idUsuario);
                    break;
            }

            return _mapper.Map<IEnumerable<TicketesDTO>>(tickets);
        }

        /// <summary>
        /// Obtiene información del usuario usando reflexión para manejar diferentes estructuras de datos
        /// </summary>
        public async Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario)
        {
            var usuario = await _usuarioRepository.FindByIdAsync(idUsuario);
            if (usuario == null)
                throw new KeyNotFoundException($"Usuario con ID {idUsuario} no encontrado");

            // Valor por defecto
            string rolNombre = "Cliente";

            // Obtener el primer rol del usuario si existe
            var usuarioRol = usuario.UsuarioRoles?.FirstOrDefault();
            if (usuarioRol != null)
            {
                try
                {
                    // Usar reflexión para obtener el nombre del rol de forma flexible
                    var urType = usuarioRol.GetType();

                    // 1) Buscar navegación al objeto rol (IdRolNavigation o Rol)
                    var navProp = urType.GetProperty("IdRolNavigation") ?? urType.GetProperty("Rol");
                    if (navProp != null)
                    {
                        var navValue = navProp.GetValue(usuarioRol);
                        if (navValue != null)
                        {
                            var navType = navValue.GetType();
                            var nombreProp = navType.GetProperty("NombreRol") ?? navType.GetProperty("Nombre");
                            if (nombreProp != null)
                            {
                                var nombre = nombreProp.GetValue(navValue) as string;
                                if (!string.IsNullOrWhiteSpace(nombre))
                                    rolNombre = nombre;
                            }
                        }
                    }

                    // 2) Si no se encontró, buscar directamente en UsuarioRol
                    if (rolNombre == "Cliente")
                    {
                        var directNombreProp = urType.GetProperty("NombreRol") ?? urType.GetProperty("Nombre");
                        if (directNombreProp != null)
                        {
                            var directNombre = directNombreProp.GetValue(usuarioRol) as string;
                            if (!string.IsNullOrWhiteSpace(directNombre))
                                rolNombre = directNombre;
                        }
                    }
                }
                catch
                {
                    // Mantener rol por defecto en caso de error
                }
            }

            return new UsuarioDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = rolNombre
            };
        }

        /// <summary>
        /// Prepara el DTO para crear un ticket con información del usuario prellenada
        /// </summary>
        public async Task<TicketCreateDTO> PrepareCreateDTOAsync(int idUsuarioSolicitante)
        {
            var usuario = await GetUsuarioInfoAsync(idUsuarioSolicitante);

            return new TicketCreateDTO
            {
                IdUsuarioSolicitante = usuario.IdUsuario,
                NombreSolicitante = usuario.Nombre,
                CorreoSolicitante = usuario.Correo,
                FechaCreacion = DateTime.Now,
                Estado = "Pendiente"
            };
        }

        /// <summary>
        /// Crea un ticket con cálculos automáticos de SLA basados en la etiqueta seleccionada
        /// </summary>
        public async Task<int> CreateTicketAsync(TicketCreateDTO dto)
        {
            // 1. Validar que la etiqueta existe
            var etiqueta = await _etiquetaRepository.FindByIdAsync(dto.IdEtiqueta);
            if (etiqueta == null)
                throw new KeyNotFoundException("La etiqueta seleccionada no existe");

            // 2. Obtener la categoría asociada a la etiqueta
            var categoria = await _categoriaRepository.FindCategoriaByEtiquetaAsync(dto.IdEtiqueta);
            if (categoria == null)
                throw new InvalidOperationException($"No se encontró una categoría asociada a la etiqueta '{etiqueta.nombre_etiqueta}'");

            // 3. Obtener el SLA de la categoría
            var categoriaSLA = categoria.CategoriaSLAs?.FirstOrDefault();
            if (categoriaSLA == null || categoriaSLA.SLA == null)
                throw new InvalidOperationException($"La categoría '{categoria.nombre_categoria}' no tiene un SLA configurado");

            var sla = categoriaSLA.SLA;

            // 4. Calcular fecha límite de resolución basada en el SLA
            DateTime fechaCreacion = DateTime.Now;
            DateTime? fechaLimiteResolucion = null;

            if (sla.tiempo_resolucion_horas.HasValue && sla.tiempo_resolucion_horas > 0)
            {
                fechaLimiteResolucion = fechaCreacion.AddHours(sla.tiempo_resolucion_horas.Value);
            }

            // 5. Crear el ticket
            var ticket = new Tickets
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                IdUsuarioSolicitante = dto.IdUsuarioSolicitante,
                IdCategoria = categoria.id_categoria,
                IdSla = sla.id_sla,
                Estado = "Pendiente",
                FechaCreacion = fechaCreacion,
                FechaActualizacion = fechaCreacion,
                FechaLimiteResolucion = fechaLimiteResolucion
            };

            // Guardar el ticket en la base de datos
            await _repository.CreateAsync(ticket);

            // 6. Registrar en el historial
            var historial = new Historial_Tickets
            {
                IdTicket = ticket.IdTicket,
                IdUsuario = dto.IdUsuarioSolicitante,
                Accion = $"Ticket creado - Categoría: '{categoria.nombre_categoria}' | Prioridad: '{sla.prioridad ?? "Media"}' | Etiqueta: '{etiqueta.nombre_etiqueta}'",
                FechaAccion = DateTime.Now
            };

            await _repository.AddHistorialAsync(historial);

            // 7. Retornar el ID del ticket creado
            return ticket.IdTicket;
        }
    }
}