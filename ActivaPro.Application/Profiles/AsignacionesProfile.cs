using ActivaPro.Application.DTOs;
using ActivaPro.Infraestructure.Models;
using AutoMapper;
using System;
using System.Globalization;
using System.Linq;

namespace ActivaPro.Application.Profiles
{
    public class AsignacionesProfile : Profile
    {
        public AsignacionesProfile()
        {
            CreateMap<AsignacionesTickets, AsignacionesDTO>()
                .ForMember(dest => dest.IdAsignacion, opt => opt.MapFrom(src => src.IdAsignacion))
                .ForMember(dest => dest.IdTicket, opt => opt.MapFrom(src => src.IdTicket))
                .ForMember(dest => dest.IdUsuarioAsignado, opt => opt.MapFrom(src => src.IdUsuarioAsignado))
                .ForMember(dest => dest.IdUsuarioAsignador, opt => opt.MapFrom(src => src.IdUsuarioAsignador))
                .ForMember(dest => dest.TipoAsignacion, opt => opt.MapFrom(src => src.TipoAsignacion))
                .ForMember(dest => dest.FechaAsignacion, opt => opt.MapFrom(src => src.FechaAsignacion))

                .ForMember(dest => dest.NombreTecnico, opt => opt.MapFrom(src =>
                    src.IdUsuarioAsignadoNavigation != null ? src.IdUsuarioAsignadoNavigation.Nombre : "Sin asignar"))
                .ForMember(dest => dest.CorreoTecnico, opt => opt.MapFrom(src =>
                    src.IdUsuarioAsignadoNavigation != null ? src.IdUsuarioAsignadoNavigation.Correo : string.Empty))

                .ForMember(dest => dest.NumeroSemana, opt => opt.MapFrom(src =>
                    src.FechaAsignacion.HasValue
                        ? CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            src.FechaAsignacion.Value,
                            CalendarWeekRule.FirstDay,
                            DayOfWeek.Monday)
                        : 0))
                .ForMember(dest => dest.Anio, opt => opt.MapFrom(src =>
                    src.FechaAsignacion.HasValue ? src.FechaAsignacion.Value.Year : DateTime.Now.Year))
                .ForMember(dest => dest.SemanaAsignacion, opt => opt.MapFrom(src =>
                    src.FechaAsignacion.HasValue
                        ? $"Semana {CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(src.FechaAsignacion.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday)} - {src.FechaAsignacion.Value.Year}"
                        : "Sin fecha"));
        }
    }
}