using ActivaPro.Application.DTOs;
using ActivaPro.Infraestructure.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivaPro.Application.Profiles
{
    public class TicketesProfile : Profile
    {
        public TicketesProfile()
        {
            CreateMap<Tickets, TicketesDTO>()
                .ForMember(dest => dest.IdTicket, opt => opt.MapFrom(src => src.IdTicket))
                .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => src.FechaActualizacion))
                .ForMember(dest => dest.IdUsuarioSolicitante, opt => opt.MapFrom(src => src.IdUsuarioSolicitante))
                .ForMember(dest => dest.IdUsuarioAsignado, opt => opt.MapFrom(src => src.IdUsuarioAsignado))
                .ForMember(dest => dest.IdCategoria, opt => opt.MapFrom(src => src.IdCategoria))
                .ForMember(dest => dest.IdValoracion, opt => opt.MapFrom(src => src.IdValoracion))
                .ForMember(dest => dest.IdSLA, opt => opt.MapFrom(src => src.IdSla))
                .ForMember(dest => dest.FechaLimiteResolucion, opt => opt.MapFrom(src => src.FechaLimiteResolucion))

                // Usuarios
                .ForMember(dest => dest.NombreSolicitante, opt => opt.MapFrom(src =>
                    src.UsuarioSolicitante != null ? src.UsuarioSolicitante.Nombre : string.Empty))
                .ForMember(dest => dest.NombreAsignado, opt => opt.MapFrom(src =>
                    src.UsuarioAsignado != null ? src.UsuarioAsignado.Nombre : "Sin asignar"))

                // Categoría y Etiquetas
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src =>
                    src.Categoria != null ? src.Categoria.nombre_categoria : "Sin categoría"))
                .ForMember(dest => dest.Etiquetas, opt => opt.MapFrom(src =>
                    src.Categoria != null && src.Categoria.CategoriaEtiquetas != null
                        ? src.Categoria.CategoriaEtiquetas
                            .Where(ce => ce.Etiqueta != null)
                            .Select(ce => ce.Etiqueta.nombre_etiqueta)
                            .Distinct()
                            .ToList()
                        : new List<string>()))

                // SLA
                .ForMember(dest => dest.SLA_Descripcion, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.descripcion : "Sin SLA"))
                .ForMember(dest => dest.SLA_Prioridad, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.prioridad : "Sin prioridad"))
                .ForMember(dest => dest.SLA_TiempoRespuestaHoras, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.tiempo_resolucion_horas : (int?)null))
                .ForMember(dest => dest.SLA_TiempoResolucionHoras, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.tiempo_resolucion_horas : (int?)null))

                // Imágenes
                .ForMember(dest => dest.Imagenes, opt => opt.MapFrom(src =>
                    src.Imagenes != null ? src.Imagenes.Select(i => new ImagenTicketDTO
                    {
                        IdImagen = i.IdImagen,
                        NombreArchivo = i.NombreArchivo,
                        RutaArchivo = i.RutaArchivo,
                        FechaSubida = i.FechaSubida
                    }).ToList() : new List<ImagenTicketDTO>()))

                // ========== HISTORIAL SIMPLE (SIN CAMPOS EXTENDIDOS) ==========
                // Mapea a HistorialTicketDetalladoDTO pero con valores vacíos en campos que no existen
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src =>
                    src.Historial != null ? src.Historial.OrderByDescending(h => h.FechaAccion).Select(h => new HistorialTicketDetalladoDTO
                    {
                        IdHistorial = h.IdHistorial,
                        NombreUsuario = h.Usuario != null ? h.Usuario.Nombre : "Usuario desconocido",
                        Accion = h.Accion ?? string.Empty,
                        EstadoAnterior = string.Empty,  // Valor por defecto si no existe
                        EstadoNuevo = string.Empty,     // Valor por defecto si no existe
                        Comentario = string.Empty,      // Valor por defecto si no existe
                        FechaAccion = h.FechaAccion,
                        ImagenesEvidencia = new List<ImagenTicketDTO>() // Lista vacía
                    }).ToList() : new List<HistorialTicketDetalladoDTO>()))

                // Valoración
                .ForMember(dest => dest.Valoracion, opt => opt.MapFrom(src =>
                    src.Valoraciones != null && src.Valoraciones.Any()
                        ? new ValoracionTicketDTO
                        {
                            IdValoracion = src.Valoraciones.First().IdValoracion,
                            Puntaje = src.Valoraciones.First().Puntaje,
                            Comentario = src.Valoraciones.First().Comentario,
                            FechaValoracion = src.Valoraciones.First().FechaValoracion
                        }
                        : null))

                // Cálculos
                .ForMember(dest => dest.DiasDesdeCreacion, opt => opt.MapFrom(src =>
                    (DateTime.Now - src.FechaCreacion).Days))
                .ForMember(dest => dest.DiasParaResolucion, opt => opt.MapFrom(src =>
                    src.FechaLimiteResolucion.HasValue
                        ? (src.FechaLimiteResolucion.Value - DateTime.Now).Days
                        : (int?)null))
                .ForMember(dest => dest.CumpleRespuesta, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CumpleResolucion, opt => opt.MapFrom(src =>
                    src.Estado == "Cerrado" && src.FechaLimiteResolucion.HasValue
                        ? src.FechaActualizacion <= src.FechaLimiteResolucion.Value
                        : (bool?)null));
        }
    }
}