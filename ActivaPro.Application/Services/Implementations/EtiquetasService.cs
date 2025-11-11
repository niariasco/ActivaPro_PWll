using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class EtiquetasService : IEtiquetasService
    {
        private readonly IRepoEtiquetas _repository;
        private readonly IMapper _mapper;

        public EtiquetasService(IRepoEtiquetas repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Etiquetas>> ListAsync()
        {
            var etiquetas = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<Etiquetas>>(etiquetas);
        }

        public async Task<Etiquetas?> FindByIdAsync(int id)
        {
            var etiqueta = await _repository.FindByIdAsync(id);
            if (etiqueta == null)
                return null;

            return _mapper.Map<Etiquetas>(etiqueta);
        }
    }
}
