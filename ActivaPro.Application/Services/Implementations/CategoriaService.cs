using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IRepoCategorias _repository;
        private readonly IMapper _mapper;

        public CategoriaService(IRepoCategorias repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoriasDTO>> ListAsync()
        {
            var categorias = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<CategoriasDTO>>(categorias);
        }

        public async Task<CategoriasDTO?> FindByIdAsync(int id)
        {
            var categoria = await _repository.FindByIdAsync(id);
            if (categoria == null)
                return null;

            return _mapper.Map<CategoriasDTO>(categoria);
        }
    }
}