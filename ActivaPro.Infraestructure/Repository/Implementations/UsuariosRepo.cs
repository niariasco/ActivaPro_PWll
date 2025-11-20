using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Implementations
{
    public class UsuariosRepo : IRepoUsuarios
    {
        private readonly ActivaProContext _ctx;
        public UsuariosRepo(ActivaProContext ctx) => _ctx = ctx;

        public Task<Usuarios?> FindByIdAsync(int id) =>
            _ctx.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

        public Task<Usuarios?> FindByCorreoAsync(string correo) =>
            _ctx.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

        public async Task CreateAsync(Usuarios usuario)
        {
            _ctx.Usuarios.Add(usuario);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuarios usuario)
        {
            _ctx.Usuarios.Update(usuario);
            await _ctx.SaveChangesAsync();
        }
    }
}