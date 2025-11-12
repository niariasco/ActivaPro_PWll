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

        // ========== CONSULTAS ==========

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


        public async Task<int> CreateTicketAsync(TicketCreateDTO dto)
        {
            return await CreateTicketAsync(dto, null);
        }

        public async Task<int> CreateTicketAsync(TicketCreateDTO dto, string rutaImagenes)
        {
            // Validar etiqueta
            var etiqueta = await _etiquetaRepository.FindByIdAsync(dto.IdEtiqueta);
            if (etiqueta == null)
                throw new KeyNotFoundException("La etiqueta seleccionada no existe");

            // Obtener categoría asociada
            var categoria = await _categoriaRepository.FindCategoriaByEtiquetaAsync(dto.IdEtiqueta);
            if (categoria == null)
                throw new InvalidOperationException($"No se encontró una categoría asociada a la etiqueta '{etiqueta.nombre_etiqueta}'");

            // Obtener SLA
            var categoriaSLA = categoria.CategoriaSLAs?.FirstOrDefault();
            if (categoriaSLA == null || categoriaSLA.SLA == null)
                throw new InvalidOperationException($"La categoría '{categoria.nombre_categoria}' no tiene un SLA configurado");

            var sla = categoriaSLA.SLA;

            // Calcular fechas
            DateTime fechaCreacion = DateTime.Now;
            DateTime? fechaLimiteResolucion = null;

            if (sla.tiempo_resolucion_horas.HasValue && sla.tiempo_resolucion_horas > 0)
            {
                fechaLimiteResolucion = fechaCreacion.AddHours(sla.tiempo_resolucion_horas.Value);
            }

            // Crear ticket
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

            // Procesar imágenes
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

            // Registrar historial
            string accionHistorial = $"Ticket creado - Categoría: '{categoria.nombre_categoria}' | Prioridad: '{sla.prioridad ?? "Media"}' | Etiqueta: '{etiqueta.nombre_etiqueta}'";
            if (cantidadImagenes > 0)
            {
                accionHistorial += $" | {cantidadImagenes} imagen(es) adjuntada(s)";
            }

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


        public async Task<TicketEditDTO> PrepareEditDTOAsync(int idTicket)
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
                }).ToList() ?? new List<ImagenTicketDTO>()
            };

            return dto;
        }

        public async Task UpdateTicketAsync(TicketEditDTO dto, string rutaImagenes, int idUsuarioActual)
        {
            var ticket = await _repository.FindByIdAsync(dto.IdTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {dto.IdTicket} no encontrado");

            // Construir registro de cambios para el historial
            var cambios = new List<string>();

            // Detectar cambios
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

            // Aplicar cambios
            ticket.Titulo = dto.Titulo;
            ticket.Descripcion = dto.Descripcion;
            ticket.Estado = dto.Estado;
            ticket.IdUsuarioAsignado = dto.IdUsuarioAsignado;
            ticket.FechaActualizacion = DateTime.Now;

            await _repository.UpdateAsync(ticket);

            // Eliminar imágenes marcadas
            if (dto.ImagenesAEliminar != null && dto.ImagenesAEliminar.Any())
            {
                foreach (var idImagen in dto.ImagenesAEliminar)
                {
                    var imagen = await _repository.FindImagenByIdAsync(idImagen);
                    if (imagen != null)
                    {
                        // Eliminar archivo físico
                        if (!string.IsNullOrEmpty(rutaImagenes))
                        {
                            var nombreArchivo = System.IO.Path.GetFileName(imagen.RutaArchivo);
                            var rutaFisica = System.IO.Path.Combine(rutaImagenes, nombreArchivo);

                            if (System.IO.File.Exists(rutaFisica))
                            {
                                System.IO.File.Delete(rutaFisica);
                            }
                        }

                        // Eliminar de BD
                        await _repository.DeleteImagenAsync(idImagen);
                        cambios.Add($"Imagen eliminada: '{imagen.NombreArchivo}'");
                    }
                }
            }

            // Agregar nuevas imágenes
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

            // Registrar en historial
            if (cambios.Any())
            {
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

        

        /// <summary>
        /// Cierra un ticket cambiando su estado a "Cerrado"
        /// </summary>
        public async Task CloseTicketAsync(int idTicket, int idUsuarioActual)
        {
            var ticket = await _repository.FindByIdAsync(idTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"Ticket con ID {idTicket} no encontrado");

            // Validar que no esté ya cerrado
            if (ticket.Estado.ToLower() == "cerrado")
                throw new InvalidOperationException($"El ticket #{idTicket} ya está cerrado");

            // Cambiar estado a Cerrado
            ticket.Estado = "Cerrado";
            ticket.FechaActualizacion = DateTime.Now;

            await _repository.UpdateAsync(ticket);

            // Registrar en historial
            var historial = new Historial_Tickets
            {
                IdTicket = ticket.IdTicket,
                IdUsuario = idUsuarioActual,
                Accion = $"Ticket cerrado - Título: '{ticket.Titulo}'",
                FechaAccion = DateTime.Now
            };

            await _repository.AddHistorialAsync(historial);
        }

        

        public async Task DeleteImagenAsync(int idImagen, string rutaFisica)
        {
            var imagen = await _repository.FindImagenByIdAsync(idImagen);
            if (imagen == null)
                throw new KeyNotFoundException($"Imagen con ID {idImagen} no encontrada");

            // Eliminar archivo físico
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            // Eliminar de BD
            await _repository.DeleteImagenAsync(idImagen);
        }
    }
}