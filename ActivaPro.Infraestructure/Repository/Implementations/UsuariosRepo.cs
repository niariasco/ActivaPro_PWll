using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class UsuariosRepo : IRepoUsuarios
    {
        private readonly ActivaProContext _context;

        public UsuariosRepo(ActivaProContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> FindByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
        }
    }
}