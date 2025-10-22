using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Repository.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Implementations
{
    public class TicketesService : ITicketesService
    {
        private readonly IRepoTicketes _repository;
        private readonly IMapper _mapper;

        public TicketesService(IRepoTicketes repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<TicketesDTO?> FindByIdAsync(int id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket == null)
                return null;

            return _mapper.Map<TicketesDTO>(ticket);
        }

        public async Task<IEnumerable<TicketesDTO>> ListAsync()
        {
            var tickets = await _repository.ListAsync();
            return _mapper.Map<IEnumerable<TicketesDTO>>(tickets);
        }

        public async Task<IEnumerable<TicketesDTO>> ListByRolAsync(int idUsuario, string rol)
        {
            ICollection<ActivaPro.Infraestructure.Models.Tickets> tickets;

            switch (rol.ToLower())
            {
                case "administrador":  // Administrador - ve todos los tickets
                    tickets = await _repository.ListAsync();
                    break;

                case "técnico":     // Técnico - ve solo los asignados a él
                    tickets = await _repository.ListByUsuarioAsignadoAsync(idUsuario);
                    break;

                case "cliente":    // Cliente - ve solo los que él creó
                    tickets = await _repository.ListByUsuarioSolicitanteAsync(idUsuario);
                    break;

                default:            // Por defecto, solo los que creó
                    tickets = await _repository.ListByUsuarioSolicitanteAsync(idUsuario);
                    break;
            }

            return _mapper.Map<IEnumerable<TicketesDTO>>(tickets);
        }
    }
}