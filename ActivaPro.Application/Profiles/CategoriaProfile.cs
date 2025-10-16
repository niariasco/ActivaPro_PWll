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
    public class CategoriaProfile : Profile
    {
        public CategoriaProfile()
        {
            CreateMap<Categorias, CategoriasDTO>()
         .ForMember(dest => dest.Etiquetas, opt => opt.MapFrom(src =>
             src.CategoriaEtiquetas.Select(e => e.nombre_etiqueta).ToList()))
         .ForMember(dest => dest.Especialidades, opt => opt.MapFrom(src =>
             src.CategoriaEspecialidades.Select(e => e.NombreEspecialidad).ToList()))
         .ForMember(dest => dest.SLA, opt => opt.MapFrom(src =>
             src.SLA_Tickets.Select(s => s.descripcion).ToList()));
        }
    }
}