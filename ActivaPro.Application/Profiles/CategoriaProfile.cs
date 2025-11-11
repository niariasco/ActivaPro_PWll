using ActivaPro.Application.DTOs;
using ActivaPro.Infraestructure.Models;
using AutoMapper;
using System.Linq;

namespace ActivaPro.Application.Profiles
{
    public class CategoriaProfile : Profile
    {
        public CategoriaProfile()
        {
            // Entity -> DTO
            CreateMap<Categorias, CategoriasDTO>()
                .ForMember(dest => dest.Etiquetas,
                    opt => opt.MapFrom(src =>
                        src.CategoriaEtiquetas != null
                            ? src.CategoriaEtiquetas.Select(ce => ce.Etiqueta.nombre_etiqueta)
                            : Enumerable.Empty<string>()))
                .ForMember(dest => dest.Especialidades,
                    opt => opt.MapFrom(src =>
                        src.CategoriaEspecialidades != null
                            ? src.CategoriaEspecialidades.Select(cs => cs.Especialidad.NombreEspecialidad)
                            : Enumerable.Empty<string>()))
                .ForMember(dest => dest.id_sla,
                    opt => opt.MapFrom(src =>
                        src.CategoriaSLAs != null
                            ? src.CategoriaSLAs.Select(sl => sl.SLA.id_sla).FirstOrDefault()
                            : (int?)null))
                .ForMember(dest => dest.SLA,
                    opt => opt.MapFrom(src =>
                        src.CategoriaSLAs != null
                            ? (src.CategoriaSLAs.Select(sl => sl.SLA.descripcion).FirstOrDefault() ?? string.Empty)
                            : string.Empty));

            CreateMap<CategoriasDTO, Categorias>()
                .ForMember(dest => dest.CategoriaEtiquetas, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaEspecialidades, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaSLAs, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaSLAs, opt => opt.Ignore()); 
        }
    }
}