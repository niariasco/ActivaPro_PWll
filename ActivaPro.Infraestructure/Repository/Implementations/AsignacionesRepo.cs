using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class AsignacionesRepo : IRepoAsignaciones
    {
        private readonly ActivaProContext _context;

        public AsignacionesRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<ICollection<AsignacionesTickets>> ListAsync()
        {
            return await _context.AsignacionesTickets
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.Categoria)
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.SLA)
                .Include(a => a.IdUsuarioAsignadoNavigation)
                .Include(a => a.IdUsuarioAsignadorNavigation)
                .OrderByDescending(a => a.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<ICollection<AsignacionesTickets>> ListByTecnicoAsync(int idUsuario)
        {
            return await _context.AsignacionesTickets
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.Categoria)
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.SLA)
                .Include(a => a.IdUsuarioAsignadoNavigation)
                .Include(a => a.IdUsuarioAsignadorNavigation)
                .Where(a => a.IdUsuarioAsignado == idUsuario)
                .OrderByDescending(a => a.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<ICollection<AsignacionesTickets>> ListByTecnicoAndWeekAsync(int idUsuario, int semana, int anio)
        {
            return await _context.AsignacionesTickets
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.Categoria)
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.SLA)
                .Include(a => a.IdUsuarioAsignadoNavigation)
                .Where(a => a.IdUsuarioAsignado == idUsuario
                    && a.FechaAsignacion.HasValue
                    && a.FechaAsignacion.Value.Year == anio
                    && CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        a.FechaAsignacion.Value,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday) == semana)
                .OrderByDescending(a => a.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<AsignacionesTickets> FindByIdAsync(int id)
        {
            return await _context.AsignacionesTickets
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.Categoria)
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.SLA)
                .Include(a => a.IdUsuarioAsignadoNavigation)
                .Include(a => a.IdUsuarioAsignadorNavigation)
                .FirstOrDefaultAsync(a => a.IdAsignacion == id);
        }

        public async Task<AsignacionesTickets> GetUltimaAsignacionByTicketAsync(int idTicket)
        {
            return await _context.AsignacionesTickets
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.Categoria)
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.SLA)
                .Include(a => a.IdUsuarioAsignadoNavigation)
                .Where(a => a.IdTicket == idTicket)
                .OrderByDescending(a => a.FechaAsignacion)
                .FirstOrDefaultAsync();
        }
    }
}