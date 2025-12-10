using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class TicketesRepo : IRepoTicketes
    {
        private readonly ActivaProContext _context;

        public TicketesRepo(ActivaProContext context)
        {
            _context = context;
        }

        // ========== CONSULTAS ==========

        public async Task<Tickets> FindByIdAsync(int id)
        {
            return await _context.Ticketes
                .Include(t => t.UsuarioSolicitante)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaEtiquetas)
                        .ThenInclude(ce => ce.Etiqueta)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaSLAs)
                        .ThenInclude(cs => cs.SLA)
                .Include(t => t.SLA)
                .Include(t => t.Imagenes)
                .Include(t => t.Valoraciones)
                .FirstOrDefaultAsync(t => t.IdTicket == id);
        }

        public async Task<ICollection<Tickets>> ListAsync()
        {
            return await _context.Ticketes
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                        .ThenInclude(ce => ce.Etiqueta)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                        .ThenInclude(cs => cs.SLA)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }

        public async Task<ICollection<Tickets>> ListByUsuarioSolicitanteAsync(int idUsuario)
        {
            return await _context.Ticketes
                 .Where(t => t.IdUsuarioSolicitante == idUsuario)
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                        .ThenInclude(ce => ce.Etiqueta)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                        .ThenInclude(cs => cs.SLA)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }

        public async Task<ICollection<Tickets>> ListByUsuarioAsignadoAsync(int idUsuario)
        {
            return await _context.Ticketes
                 .Where(t => t.IdUsuarioAsignado == idUsuario)
                 .Include(t => t.UsuarioSolicitante)
                 .Include(t => t.UsuarioAsignado)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaEtiquetas)
                        .ThenInclude(ce => ce.Etiqueta)
                 .Include(t => t.Categoria)
                     .ThenInclude(c => c.CategoriaSLAs)
                        .ThenInclude(cs => cs.SLA)
                 .Include(t => t.SLA)
                 .OrderByDescending(t => t.FechaCreacion)
                 .ToListAsync();
        }

        public async Task<ICollection<Tickets>> ListByEstadoAsync(string estado)
        {
            return await _context.Ticketes
                .Where(t => t.Estado == estado)
                .Include(t => t.UsuarioSolicitante)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaEtiquetas)
                        .ThenInclude(ce => ce.Etiqueta)
                .Include(t => t.Categoria)
                    .ThenInclude(c => c.CategoriaSLAs)
                        .ThenInclude(cs => cs.SLA)
                .Include(t => t.SLA)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        // ========== CREACIÓN ==========

        public async Task CreateAsync(Tickets ticket)
        {
            _context.Ticketes.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task AddHistorialAsync(Historial_Tickets historial)
        {
            _context.Historial_Tickets.Add(historial);
            await _context.SaveChangesAsync();
        }

        public async Task AddImagenAsync(Imagenes_Tickets imagen)
        {
            _context.Imagenes_Tickets.Add(imagen);
            await _context.SaveChangesAsync();
        }

        // ========== ACTUALIZACIÓN - ⭐ CORREGIDO ==========

        /// <summary>
        /// Actualiza un ticket existente
        /// CORREGIDO: Evita conflictos de tracking y problemas con navegaciones
        /// </summary>
        public async Task UpdateAsync(Tickets ticket)
        {
            try
            {
                // ⭐ SOLUCIÓN: Obtener el ticket sin tracking y actualizar solo los campos necesarios
                var ticketExistente = await _context.Ticketes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.IdTicket == ticket.IdTicket);

                if (ticketExistente == null)
                {
                    throw new KeyNotFoundException($"Ticket con ID {ticket.IdTicket} no encontrado");
                }

                // ⭐ IMPORTANTE: Detach cualquier instancia tracked
                var trackedEntity = _context.ChangeTracker.Entries<Tickets>()
                    .FirstOrDefault(e => e.Entity.IdTicket == ticket.IdTicket);

                if (trackedEntity != null)
                {
                    trackedEntity.State = EntityState.Detached;
                }

                // ⭐ IMPORTANTE: Limpiar las navegaciones para evitar conflictos
                ticket.UsuarioSolicitante = null;
                ticket.UsuarioAsignado = null;
                ticket.Categoria = null;
                ticket.SLA = null;
                ticket.Imagenes = null;
                ticket.Historial = null;
                ticket.Valoraciones = null;

                // Marcar solo los campos que queremos actualizar
                _context.Ticketes.Attach(ticket);
                _context.Entry(ticket).Property(t => t.Estado).IsModified = true;
                _context.Entry(ticket).Property(t => t.FechaActualizacion).IsModified = true;
                _context.Entry(ticket).Property(t => t.Titulo).IsModified = true;
                _context.Entry(ticket).Property(t => t.Descripcion).IsModified = true;
                _context.Entry(ticket).Property(t => t.IdUsuarioAsignado).IsModified = true;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DbUpdateException en UpdateAsync:");
                System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                throw new InvalidOperationException($"Error al actualizar el ticket: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception en UpdateAsync:");
                System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException($"Error inesperado al actualizar el ticket: {ex.Message}", ex);
            }
        }

        // ========== GESTIÓN DE IMÁGENES ==========

        public async Task<Imagenes_Tickets> FindImagenByIdAsync(int idImagen)
        {
            return await _context.Imagenes_Tickets
                .FirstOrDefaultAsync(i => i.IdImagen == idImagen);
        }

        public async Task DeleteImagenAsync(int idImagen)
        {
            var imagen = await _context.Imagenes_Tickets
                .FirstOrDefaultAsync(i => i.IdImagen == idImagen);

            if (imagen != null)
            {
                _context.Imagenes_Tickets.Remove(imagen);
                await _context.SaveChangesAsync();
            }
        }

        // ========== HISTORIAL ==========

        public async Task<ICollection<Historial_Tickets>> GetHistorialByTicketIdAsync(int idTicket)
        {
            return await _context.Historial_Tickets
                .Where(h => h.IdTicket == idTicket)
                .Include(h => h.IdUsuarioNavigation)
                .OrderByDescending(h => h.FechaAccion)
                .ToListAsync();
        }
    }
}