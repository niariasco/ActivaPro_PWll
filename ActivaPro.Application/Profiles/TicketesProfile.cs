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

                .ForMember(dest => dest.NombreSolicitante, opt => opt.MapFrom(src =>
                    src.UsuarioSolicitante != null ? src.UsuarioSolicitante.Nombre : string.Empty))
                .ForMember(dest => dest.NombreAsignado, opt => opt.MapFrom(src =>
                    src.UsuarioAsignado != null ? src.UsuarioAsignado.Nombre : "Sin asignar"))

                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src =>
                    src.Categoria != null ? src.Categoria.nombre_categoria : "Sin categoría"))
                .ForMember(dest => dest.Etiquetas, opt => opt.MapFrom(src =>
                    src.Categoria != null && src.Categoria.CategoriaEtiquetas != null
                        ? src.Categoria.CategoriaEtiquetas.Select(e => e.nombre_etiqueta).ToList()
                        : new List<string>()))

                .ForMember(dest => dest.SLA_Descripcion, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.descripcion : "Sin SLA"))
                .ForMember(dest => dest.SLA_Prioridad, opt => opt.MapFrom(src =>
                    src.SLA != null ? src.SLA.prioridad : "Sin prioridad"))

                // Mapear imágenes
                .ForMember(dest => dest.Imagenes, opt => opt.MapFrom(src =>
                    src.Imagenes != null ? src.Imagenes.Select(i => new ImagenTicketDTO
                    {
                        IdImagen = i.IdImagen,
                        NombreArchivo = i.NombreArchivo,
                        RutaArchivo = i.RutaArchivo,
                        FechaSubida = i.FechaSubida
                    }).ToList() : new List<ImagenTicketDTO>()))

                // Mapear historial
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src =>
                    src.Historial != null ? src.Historial.OrderByDescending(h => h.FechaAccion).Select(h => new HistorialTicketDTO
                    {
                        IdHistorial = h.IdHistorial,
                        NombreUsuario = h.Usuario != null ? h.Usuario.Nombre : "Usuario desconocido",
                        Accion = h.Accion,
                        FechaAccion = h.FechaAccion
                    }).ToList() : new List<HistorialTicketDTO>()))

                // Mapear valoración
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