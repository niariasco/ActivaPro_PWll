using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class TecnicoService : ITecnicoService
    {
        private readonly IRepoTecnico _repository;
        private readonly IMapper _mapper;
        private readonly ActivaProContext _context;
        public TecnicoService(ActivaProContext context, IRepoTecnico repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ICollection<TecnicosDTO>> ListAsync()
        {
            //var list = await _repository.ListAsync();
            //return _mapper.Map<ICollection<TecnicosDTO>>(list);
            var tecnicos = await _context.Tecnico
                                    .Include(t => t.Usuario)
                                    .ToListAsync();

            return _mapper.Map<List<TecnicosDTO>>(tecnicos);
        }

        public async Task<TecnicosDTO?> FindByIdAsync(int id)
        {
            var tecnico = await _repository.FindByIdAsync(id);
            if (tecnico == null)
                return null;

            return _mapper.Map<TecnicosDTO>(tecnico);
        }
    }
}