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
                .Include(a => a.IdTicketNavigation)
                    .ThenInclude(t => t.UsuarioSolicitante)
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

        public async Task<AsignacionesTickets> AddAsync(AsignacionesTickets asignacion)
        {
            await _context.AsignacionesTickets.AddAsync(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }

        public async Task<AsignacionesTickets> UpdateAsync(AsignacionesTickets asignacion)
        {
            _context.AsignacionesTickets.Update(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asignacion = await FindByIdAsync(id);
            if (asignacion == null) return false;

            _context.AsignacionesTickets.Remove(asignacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAsignacionActivaAsync(int idTicket)
        {
            return await _context.AsignacionesTickets
                .AnyAsync(a => a.IdTicket == idTicket
                    && (a.IdTicketNavigation.Estado == "Asignado"
                        || a.IdTicketNavigation.Estado == "En Proceso"));
        }

        public async Task<int> CountTicketsActivosByTecnicoAsync(int idTecnico)
        {
            return await _context.AsignacionesTickets
                .Where(a => a.IdUsuarioAsignado == idTecnico
                    && (a.IdTicketNavigation.Estado == "Asignado"
                        || a.IdTicketNavigation.Estado == "En Proceso"))
                .CountAsync();
        }

        public async Task<int> CountTicketsPendientesByTecnicoAsync(int idTecnico)
        {
            return await _context.AsignacionesTickets
                .Where(a => a.IdUsuarioAsignado == idTecnico
                    && a.IdTicketNavigation.Estado == "Asignado")
                .CountAsync();
        }

        public async Task<int> CountTicketsEnProcesoByTecnicoAsync(int idTecnico)
        {
            return await _context.AsignacionesTickets
                .Where(a => a.IdUsuarioAsignado == idTecnico
                    && a.IdTicketNavigation.Estado == "En Proceso")
                .CountAsync();
        }
    }
}