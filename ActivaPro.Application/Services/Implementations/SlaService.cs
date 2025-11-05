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
    public class SlaService : ISlaService
    {
        private readonly IRepoSLA_Tickets _repository;
        private readonly IMapper _mapper;

        public SlaService(IRepoSLA_Tickets repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SLA_Tickets>> ListAsync()
        {
            var slas = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<SLA_Tickets>>(slas);
        }

        public async Task<SLA_Tickets?> FindByIdAsync(int id)
        {
            var sla = await _repository.FindByIdAsync(id);
            if (sla == null)
                return null;

            return _mapper.Map<SLA_Tickets>(sla);
        }
    }
}
