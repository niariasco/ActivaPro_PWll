using ActivaPro.Application.DTOs;
using ActivaPro.Infraestructure.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Profiles
{
    public class TecnicoProfile : Profile
    {
        public TecnicoProfile()
        {
            // CreateMap<TecnicosDTO, Tecnicos>().ReverseMap();
            /*   CreateMap<Tecnicos, TecnicosDTO>()
                   .ForMember(dest => dest.Especialidades, opt => opt.MapFrom(src =>
                       src.UsuarioEspecialidades.Select(e => e.Especialidad.NombreEspecialidad).ToList()
                   ));
            */

            CreateMap<Tecnicos, TecnicosDTO>()
       .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.Nombre))
       .ForMember(dest => dest.CorreoUsuario, opt => opt.MapFrom(src => src.Usuario.Correo))
       .ReverseMap();

        }
    }
}