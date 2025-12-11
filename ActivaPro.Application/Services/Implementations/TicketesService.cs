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
    public class TicketesService : ITicketesService
    {
        private readonly IRepoTicketes _repository;
        private readonly IRepoUsuarios _usuarioRepository;
        private readonly IRepoEtiquetas _etiquetaRepository;
        private readonly IRepoCategorias _categoriaRepository;
        private readonly IMapper _mapper;
        private readonly INotificacionService _notificacionService;
        private readonly IRepoValoraciones _valoracionRepository;

        public TicketesService(
            IRepoTicketes repository,
            IRepoUsuarios usuarioRepository,
            IRepoEtiquetas etiquetaRepository,
            IRepoCategorias categoriaRepository,
             IRepoValoraciones valoracionRepository,
            IMapper mapper,
            INotificacionService notificacionService)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _etiquetaRepository = etiquetaRepository;
            _categoriaRepository = categoriaRepository;
            _valoracionRepository = valoracionRepository;
            _mapper = mapper;
            _notificacionService = notificacionService;
        }

        // ========== CONSULTAS ==========

        public async Task<TicketesDTO?> FindByIdAsync(int id)
        {
            // 1️⃣ Obtener el ticket
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket == null)
                return null;

            // 2️⃣ Mapear el ticket (sin historial ni valoración)
            var ticketDTO = _mapper.Map<TicketesDTO>(ticket);

            // 3️⃣ Obtener el historial directamente de la base de datos
            var historialEntidades = await _repository.GetHistorialByTicketIdAsync(id);

            if (historialEntidades != null && historialEntidades.Any())
            {
                ticketDTO.Historial = _mapper.Map<List<HistorialTicketDetalladoDTO>>(historialEntidades);
            }
            else
            {
                ticketDTO.Historial = new List<HistorialTicketDetalladoDTO>();
            }

            // 4️⃣ ⭐ OBTENER LA VALORACIÓN SI EXISTE
            if (ticket.Estado == "Cerrado")
            {
                var valoracion = await _valoracionRepository.FindByTicketIdAsync(id);

                if (valoracion != null)
                {
                    ticketDTO.Valoracion = new ValoracionTicketDTO
                    {
                        IdValoracion = valoracion.IdValoracion,
                        Puntaje = valoracion.Puntaje,
                        Comentario = valoracion.Comentario ?? string.Empty,
                        FechaValoracion = valoracion.FechaValoracion
                    };
                }
            }

            return ticketDTO;
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
                case "coordinador":
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
                    tickets = await _repository.ListByUsuarioSolicitanteAsync(idUsuario);
                    break;
            }

            return _mapper.Map<IEnumerable<TicketesDTO>>(tickets);
        }

        public async Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario)
        {
            var usuario = await _usuarioRepository.FindByIdAsync(idUsuario);
            if (usuario == null)
                throw new KeyNotFoundException($"Usuario con ID {idUsuario} no encontrado");

            string rolNombre = "Cliente";
            var usuarioRol = usuario.UsuarioRoles?.FirstOrDefault();

            if (usuarioRol != null)
            {
                try
                {
                    var urType = usuarioRol.GetType();
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
                    // Mantener rol por defecto
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

        // ========== CREACIÓN ==========

        public async Task<int> CreateTicketAsync(TicketCreateDTO dto)
        {
            return await CreateTicketAsync(dto, null);
        }

        public async Task<int> CreateTicketAsync(TicketCreateDTO dto, string rutaImagenes)
        {
            var etiqueta = await _etiquetaRepository.FindByIdAsync(dto.IdEtiqueta);
            if (etiqueta == null)
                throw new KeyNotFoundException("La etiqueta seleccionada no existe");

            var categoria = await _categoriaRepository.FindCategoriaByEtiquetaAsync(dto.IdEtiqueta);
            if (categoria == null)
                throw new InvalidOperationException($"No se encontró una categoría asociada a la etiqueta '{etiqueta.nombre_etiqueta}'");

            var categoriaSLA = categoria.CategoriaSLAs?.FirstOrDefault();
            if (categoriaSLA == null || categoriaSLA.SLA == null)
                throw new InvalidOperationException($"La categoría '{categoria.nombre_categoria}' no tiene un SLA configurado");

            var sla = categoriaSLA.SLA;

            DateTime fechaCreacion = DateTime.Now;
            DateTime? fechaLimiteResolucion = null;

            if (sla.tiempo_resolucion_horas.HasValue && sla.tiempo_resolucion_horas > 0)
            {
                fechaLimiteResolucion = fechaCreacion.AddHours(sla.tiempo_resolucion_horas.Value);
            }

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

            await _repository.CreateAsync(ticket);

            var usuariosDestino = new List<int> { ticket.IdUsuarioSolicitante };
            if (ticket.IdUsuarioAsignado.HasValue)
                usuariosDestino.Add(ticket.IdUsuarioAsignado.Value);

            await _notificacionService.CrearCambioEstadoTicketAsync(
                usuariosDestino,
                ticket.IdTicket,
                "N/A",
                ticket.Estado,
                dto.NombreSolicitante ?? "Sistema",
                "Creación del ticket");

            int cantidadImagenes = 0;
            if (dto.ImagenesAdjuntas != null && dto.ImagenesAdjuntas.Any() && !string.IsNullOrEmpty(rutaImagenes))
            {
                foreach (var imagenFile in dto.ImagenesAdjuntas)
                {
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        string extension = System.IO.Path.GetExtension(imagenFile.FileName);
                        string nombreUnico = $"ticket_{ticket.IdTicket}_{Guid.NewGuid()}{extension}";
                        string rutaCompleta = System.IO.Path.Combine(rutaImagenes, nombreUnico);

                        using (var stream = new System.IO.FileStream(rutaCompleta, System.IO.FileMode.Create))
                        {
                            await imagenFile.CopyToAsync(stream);
                        }

                        var imagen = new Imagenes_Tickets
                        {
                            IdTicket = ticket.IdTicket,
                            NombreArchivo = imagenFile.FileName,
                            RutaArchivo = $"/uploads/tickets/{nombreUnico}",
                            FechaSubida = DateTime.Now
                        };

                        await _repository.AddImagenAsync(imagen);
                        cantidadImagenes++;
                    }
                }
            }

            string accionHistorial = $"Ticket creado - Categoría: '{categoria.nombre_categoria}' | Prioridad: '{sla.prioridad ?? "Media"}' | Etiqueta: '{etiqueta.nombre_etiqueta}'";
            if (cantidadImagenes > 0)
            {
                accionHistorial += $" | {cantidadImagenes} imagen(es) adjuntada(s)";
            }

            // ⭐ CORREGIDO: Usar HistorialTickets (sin guion bajo)
            var historial = new Historial_Tickets
            {
                IdTicket = ticket.IdTicket,
                IdUsuario = dto.IdUsuarioSolicitante,
                Accion = accionHistorial,
                FechaAccion = DateTime.Now
            };

            await _repository.AddHistorialAsync(historial);

            return ticket.IdTicket;
        }

        // ========== ⭐ CAMBIO RÁPIDO DE ESTADO CON COMENTARIO OBLIGATORIO ==========

        /// <summary>
        /// Cambia el estado del ticket de forma rápida siguiendo el flujo secuencial
        /// INCLUYE COMENTARIO OBLIGATORIO del técnico
        /// FLUJO COMPLETO: Pendiente → Asignado → En Proceso → Resuelto → Cerrado
        /// </summary>
        public async Task CambiarEstadoRapidoAsync(int idTicket, string nuevoEstado, int idUsuarioActual, string comentario)
        {
            // ⭐ VALIDACIÓN DEL COMENTARIO
            if (string.IsNullOrWhiteSpace(comentario))
            {
                throw new ArgumentException("El comentario es obligatorio para registrar el cambio de estado.");
            }

            if (comentario.Length < 10)
            {
                throw new ArgumentException("El comentario debe tener al menos 10 caracteres.");
            }

            if (comentario.Length > 500)
            {
                throw new ArgumentException("El comentario no puede exceder 500 caracteres.");
            }

            // Cargar ticket con navegaciones para validaciones
            var ticketCompleto = await _repository.FindByIdAsync(idTicket);
            if (ticketCompleto == null)
                throw new KeyNotFoundException($"Ticket con ID {idTicket} no encontrado");

            // VALIDACIÓN 1: No cambiar si está cerrado o cancelado
            if (ticketCompleto.Estado == "Cerrado" || ticketCompleto.Estado == "Cancelado")
            {
                throw new InvalidOperationException($"No se puede cambiar el estado de un ticket '{ticketCompleto.Estado}'.");
            }

            // VALIDACIÓN 2: Verificar si ya está en ese estado
            if (ticketCompleto.Estado == nuevoEstado)
            {
                throw new InvalidOperationException($"El ticket ya está en estado '{nuevoEstado}'.");
            }

            // VALIDACIÓN 3: ⭐ VALIDAR FLUJO SECUENCIAL COMPLETO
            var estadoPermitido = TicketEditDTO.ObtenerSiguienteEstadoPermitido(ticketCompleto.Estado);

            if (estadoPermitido == null)
            {
                if (ticketCompleto.Estado == "Resuelto")
                {
                    throw new InvalidOperationException(
                        "El ticket está en estado 'Resuelto'. Solo un administrador o el cliente pueden cerrarlo.");
                }
                throw new InvalidOperationException(
                    $"El ticket en estado '{ticketCompleto.Estado}' no puede avanzar más.");
            }

            if (nuevoEstado != estadoPermitido)
            {
                throw new InvalidOperationException(
                    $"Flujo inválido. Desde '{ticketCompleto.Estado}' solo puedes cambiar a '{estadoPermitido}'.");
            }

            // VALIDACIÓN 4: Para "Asignado", verificar que el técnico esté asignado
            if (nuevoEstado == "Asignado")
            {
                if (!ticketCompleto.IdUsuarioAsignado.HasValue)
                {
                    // Asignar automáticamente al técnico actual
                    ticketCompleto.IdUsuarioAsignado = idUsuarioActual;
                }
                else if (ticketCompleto.IdUsuarioAsignado.Value != idUsuarioActual)
                {
                    throw new InvalidOperationException(
                        $"Este ticket ya está asignado a otro técnico (ID: {ticketCompleto.IdUsuarioAsignado.Value}).");
                }
            }

            // VALIDACIÓN 5: Para estados después de "Asignado", verificar asignación
            if (nuevoEstado != "Asignado")
            {
                if (!ticketCompleto.IdUsuarioAsignado.HasValue)
                {
                    throw new InvalidOperationException("El ticket debe estar asignado antes de cambiar a este estado.");
                }

                if (ticketCompleto.IdUsuarioAsignado.Value != idUsuarioActual)
                {
                    throw new InvalidOperationException("Solo puedes cambiar el estado de tickets asignados a ti.");
                }
            }

            var estadoAnterior = ticketCompleto.Estado;

            // ⭐ CREAR UN NUEVO OBJETO TICKET SIN NAVEGACIONES
            var ticketParaActualizar = new Tickets
            {
                IdTicket = ticketCompleto.IdTicket,
                Titulo = ticketCompleto.Titulo,
                Descripcion = ticketCompleto.Descripcion,
                IdUsuarioSolicitante = ticketCompleto.IdUsuarioSolicitante,
                IdUsuarioAsignado = ticketCompleto.IdUsuarioAsignado ?? idUsuarioActual,
                Estado = nuevoEstado,
                IdValoracion = ticketCompleto.IdValoracion,
                IdCategoria = ticketCompleto.IdCategoria,
                FechaCreacion = ticketCompleto.FechaCreacion,
                FechaActualizacion = DateTime.Now,
                IdSla = ticketCompleto.IdSla,
                FechaLimiteResolucion = ticketCompleto.FechaLimiteResolucion
            };

            // ⭐ GUARDAR EL TICKET
            try
            {
                await _repository.UpdateAsync(ticketParaActualizar);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al actualizar el ticket: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            // ⭐ REGISTRAR EN HISTORIAL CON EL COMENTARIO DEL TÉCNICO
            try
            {
                string emoji = nuevoEstado switch
                {
                    "Asignado" => "👤",
                    "En Proceso" => "⚙️",
                    "Resuelto" => "✅",
                    "Cerrado" => "🔒",
                    _ => "📝"
                };

                // Acción descriptiva
                string accionDescripcion = $"{emoji} Cambio de estado: '{estadoAnterior}' → '{nuevoEstado}'";
                if (accionDescripcion.Length > 255)
                {
                    accionDescripcion = accionDescripcion.Substring(0, 252) + "...";
                }

                // Validar longitudes según BD
                string estadoAnt = estadoAnterior?.Length > 50 ? estadoAnterior.Substring(0, 50) : estadoAnterior;
                string estadoNvo = nuevoEstado?.Length > 50 ? nuevoEstado.Substring(0, 50) : nuevoEstado;

                // ⭐ USAR EL COMENTARIO DEL TÉCNICO (ya viene validado)
                string comentarioFinal = comentario.Length > 500 ? comentario.Substring(0, 500) : comentario;

                var historial = new Historial_Tickets
                {
                    IdTicket = ticketCompleto.IdTicket,
                    IdUsuario = idUsuarioActual,
                    Accion = accionDescripcion,
                    EstadoAnterior = estadoAnt,
                    EstadoNuevo = estadoNvo,
                    Comentario = comentarioFinal,  // ⭐ COMENTARIO DEL TÉCNICO
                    FechaAccion = DateTime.Now
                };

                await _repository.AddHistorialAsync(historial);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al registrar historial: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"⚠️ InnerException: {ex.InnerException?.Message}");
                // No lanzar excepción - el cambio de estado ya se guardó
            }

            // ⭐ NOTIFICAR CAMBIO DE ESTADO
            try
            {
                var usuariosDestino = new List<int> { ticketCompleto.IdUsuarioSolicitante };
                if (ticketCompleto.IdUsuarioAsignado.HasValue)
                    usuariosDestino.Add(ticketCompleto.IdUsuarioAsignado.Value);

                await _notificacionService.CrearCambioEstadoTicketAsync(
                    usuariosDestino,
                    ticketCompleto.IdTicket,
                    estadoAnterior,
                    nuevoEstado,
                    idUsuarioActual.ToString(),
                    comentario);  // ⭐ PASAR EL COMENTARIO EN LA NOTIFICACIÓN
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al enviar notificación: {ex.Message}");
            }
        }
        // ========== PREPARAR EDICIÓN ==========

        public async Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket, string rolUsuario)
        {
            var ticket = await _repository.FindByIdAsync(idTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {idTicket} no encontrado");

            var dto = new TicketEditDTO
            {
                IdTicket = ticket.IdTicket,
                Titulo = ticket.Titulo,
                Descripcion = ticket.Descripcion,
                Estado = ticket.Estado,
                IdUsuarioSolicitante = ticket.IdUsuarioSolicitante,
                NombreSolicitante = ticket.UsuarioSolicitante?.Nombre,
                CorreoSolicitante = ticket.UsuarioSolicitante?.Correo,
                IdUsuarioAsignado = ticket.IdUsuarioAsignado,
                NombreUsuarioAsignado = ticket.UsuarioAsignado?.Nombre ?? "Sin asignar",
                FechaCreacion = ticket.FechaCreacion,
                FechaActualizacion = ticket.FechaActualizacion,
                IdCategoria = ticket.IdCategoria,
                CategoriaNombre = ticket.Categoria?.nombre_categoria,
                IdSLA = ticket.IdSla,
                SLA_Descripcion = ticket.SLA?.descripcion,
                SLA_Prioridad = ticket.SLA?.prioridad,
                FechaLimiteResolucion = ticket.FechaLimiteResolucion,
                ImagenesExistentes = ticket.Imagenes?.Select(i => new ImagenTicketDTO
                {
                    IdImagen = i.IdImagen,
                    NombreArchivo = i.NombreArchivo,
                    RutaArchivo = i.RutaArchivo,
                    FechaSubida = i.FechaSubida
                }).ToList() ?? new List<ImagenTicketDTO>(),

                EstadosDisponibles = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuario, ticket.Estado)
            };

            return dto;
        }

        public async Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket)
        {
            return await PrepareEditDTOAsync(idTicket, "técnico");
        }

        // ========== ACTUALIZACIÓN ==========

        public async Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual, string rolUsuario)
        {
            var ticket = await _repository.FindByIdAsync(dto.IdTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {dto.IdTicket} no encontrado");

            var estadosPermitidos = TicketEditDTO.ObtenerEstadosSegunRol(rolUsuario, ticket.Estado);
            if (!estadosPermitidos.Contains(dto.Estado))
            {
                throw new InvalidOperationException(
                    $"El estado '{dto.Estado}' no está permitido para el rol '{rolUsuario}'. " +
                    $"Estados permitidos: {string.Join(", ", estadosPermitidos)}");
            }

            var cambios = new List<string>();

            if (ticket.Titulo != dto.Titulo)
                cambios.Add($"Título: '{ticket.Titulo}' → '{dto.Titulo}'");

            if (ticket.Descripcion != dto.Descripcion)
                cambios.Add("Descripción modificada");

            if (ticket.Estado != dto.Estado)
                cambios.Add($"Estado: '{ticket.Estado}' → '{dto.Estado}'");

            if (ticket.IdUsuarioAsignado != dto.IdUsuarioAsignado)
            {
                var nombreAnterior = ticket.UsuarioAsignado?.Nombre ?? "Sin asignar";
                var nombreNuevo = dto.NombreUsuarioAsignado ?? "Sin asignar";
                cambios.Add($"Asignado: '{nombreAnterior}' → '{nombreNuevo}'");
            }

            var estadoAnterior = ticket.Estado;
            var asignadoAnterior = ticket.IdUsuarioAsignado;

            ticket.Titulo = dto.Titulo;
            ticket.Descripcion = dto.Descripcion;
            ticket.Estado = dto.Estado;
            ticket.IdUsuarioAsignado = dto.IdUsuarioAsignado;
            ticket.FechaActualizacion = DateTime.Now;

            await _repository.UpdateAsync(ticket);

            if (estadoAnterior != ticket.Estado)
            {
                var usuariosDestino = new List<int> { ticket.IdUsuarioSolicitante };
                if (ticket.IdUsuarioAsignado.HasValue)
                    usuariosDestino.Add(ticket.IdUsuarioAsignado.Value);

                await _notificacionService.CrearCambioEstadoTicketAsync(
                    usuariosDestino,
                    ticket.IdTicket,
                    estadoAnterior,
                    ticket.Estado,
                    idUsuarioActual.ToString(),
                    "Actualización de estado");
            }

            if (asignadoAnterior != ticket.IdUsuarioAsignado && ticket.IdUsuarioAsignado.HasValue)
            {
                await _notificacionService.CrearCambioEstadoTicketAsync(
                    new[] { ticket.IdUsuarioAsignado.Value },
                    ticket.IdTicket,
                    estadoAnterior,
                    ticket.Estado,
                    idUsuarioActual.ToString(),
                    "Nuevo técnico asignado");
            }

            if (dto.ImagenesAEliminar != null && dto.ImagenesAEliminar.Any())
            {
                foreach (var idImagen in dto.ImagenesAEliminar)
                {
                    var imagen = await _repository.FindImagenByIdAsync(idImagen);
                    if (imagen != null)
                    {
                        if (!string.IsNullOrEmpty(rutaImagenes))
                        {
                            var nombreArchivo = System.IO.Path.GetFileName(imagen.RutaArchivo);
                            var rutaFisica = System.IO.Path.Combine(rutaImagenes, nombreArchivo);

                            if (System.IO.File.Exists(rutaFisica))
                            {
                                System.IO.File.Delete(rutaFisica);
                            }
                        }

                        await _repository.DeleteImagenAsync(idImagen);
                        cambios.Add($"Imagen eliminada: '{imagen.NombreArchivo}'");
                    }
                }
            }

            int imagenesAgregadas = 0;
            if (dto.NuevasImagenes != null && dto.NuevasImagenes.Any() && !string.IsNullOrEmpty(rutaImagenes))
            {
                foreach (var imagenFile in dto.NuevasImagenes)
                {
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        string extension = System.IO.Path.GetExtension(imagenFile.FileName);
                        string nombreUnico = $"ticket_{ticket.IdTicket}_{Guid.NewGuid()}{extension}";
                        string rutaCompleta = System.IO.Path.Combine(rutaImagenes, nombreUnico);

                        using (var stream = new System.IO.FileStream(rutaCompleta, System.IO.FileMode.Create))
                        {
                            await imagenFile.CopyToAsync(stream);
                        }

                        var imagen = new Imagenes_Tickets
                        {
                            IdTicket = ticket.IdTicket,
                            NombreArchivo = imagenFile.FileName,
                            RutaArchivo = $"/uploads/tickets/{nombreUnico}",
                            FechaSubida = DateTime.Now
                        };

                        await _repository.AddImagenAsync(imagen);
                        imagenesAgregadas++;
                    }
                }

                if (imagenesAgregadas > 0)
                    cambios.Add($"{imagenesAgregadas} imagen(es) agregada(s)");
            }

            if (cambios.Any())
            {
                // ⭐ CORREGIDO: Usar HistorialTickets (sin guion bajo)
                var historial = new Historial_Tickets
                {
                    IdTicket = ticket.IdTicket,
                    IdUsuario = idUsuarioActual,
                    Accion = $"Ticket actualizado: {string.Join(" | ", cambios)}",
                    FechaAccion = DateTime.Now
                };

                await _repository.AddHistorialAsync(historial);
            }
        }

        public async Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual)
        {
            await UpdateTicketAsync(dto, rutaImagenes, idUsuarioActual, "técnico");
        }

        // ========== CIERRE ==========

        public async Task CloseTicketAsync(int idTicket, int idUsuarioActual)
        {
            var ticket = await _repository.FindByIdAsync(idTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {idTicket} no encontrado");

            if (ticket.Estado.ToLower() == "cerrado")
                throw new InvalidOperationException($"El ticket #{idTicket} ya está cerrado");

            var estadoAnterior = ticket.Estado;
            ticket.Estado = "Cerrado";
            ticket.FechaActualizacion = DateTime.Now;

            await _repository.UpdateAsync(ticket);

            var usuariosDestino = new List<int> { ticket.IdUsuarioSolicitante };
            if (ticket.IdUsuarioAsignado.HasValue)
                usuariosDestino.Add(ticket.IdUsuarioAsignado.Value);

            await _notificacionService.CrearCambioEstadoTicketAsync(
                usuariosDestino,
                ticket.IdTicket,
                estadoAnterior,
                ticket.Estado,
                idUsuarioActual.ToString(),
                "Cierre del ticket");

            // ⭐ CORREGIDO: Usar HistorialTickets (sin guion bajo)
            var historial = new Historial_Tickets
            {
                IdTicket = ticket.IdTicket,
                IdUsuario = idUsuarioActual,
                Accion = $" Ticket cerrado - Título: '{ticket.Titulo}'",
                FechaAccion = DateTime.Now
            };

            await _repository.AddHistorialAsync(historial);
        }

        // ========== GESTIÓN DE IMÁGENES ==========

        public async Task DeleteImagenAsync(int idImagen, string rutaFisica)
        {
            var imagen = await _repository.FindImagenByIdAsync(idImagen);
            if (imagen == null)
                throw new KeyNotFoundException($"Imagen con ID {idImagen} no encontrada");

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            await _repository.DeleteImagenAsync(idImagen);
        }
    }
}