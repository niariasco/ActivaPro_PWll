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
    public class EspecialidadesService : IEspecialidadesService
    {
        private readonly IRepoEspecialidades _repository;
        private readonly IMapper _mapper;

        public EspecialidadesService(IRepoEspecialidades repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Especialidades>> ListAsync()
        {
            var especialidades = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<Especialidades>>(especialidades);
        }

        public async Task<Especialidades?> FindByIdAsync(int id)
        {
            var especialidad = await _repository.FindByIdAsync(id);
            if (especialidad == null)
                return null;

            return _mapper.Map<Especialidades>(especialidad);
        }
    }
}