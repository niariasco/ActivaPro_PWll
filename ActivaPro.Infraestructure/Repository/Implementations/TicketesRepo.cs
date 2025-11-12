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
                .Include(t => t.Historial)
                    .ThenInclude(h => h.Usuario)
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

        // ========== ACTUALIZACIÓN ==========

        /// <summary>
        /// Actualiza un ticket existente (incluye cambio de estado a "Cerrado")
        /// </summary>
        public async Task UpdateAsync(Tickets ticket)
        {
            _context.Ticketes.Update(ticket);
            await _context.SaveChangesAsync();
        }

        // ========== GESTIÓN DE IMÁGENES ==========

        /// <summary>
        /// Busca una imagen específica por ID
        /// </summary>
        public async Task<Imagenes_Tickets> FindImagenByIdAsync(int idImagen)
        {
            return await _context.Imagenes_Tickets
                .FirstOrDefaultAsync(i => i.IdImagen == idImagen);
        }

        /// <summary>
        /// Elimina una imagen específica
        /// </summary>
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
    }
}