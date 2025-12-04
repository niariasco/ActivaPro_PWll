using System;
using System.Linq;
using System.Threading.Tasks;
using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using ActivaPro.Application.Security;
using ActivaPro.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IRepoUsuarios _usuarios;
        private readonly INotificacionService _notifs;
        private readonly ActivaProContext _context;

        public AuthService(IRepoUsuarios usuarios, INotificacionService notifs, ActivaProContext context)
        {
            _usuarios = usuarios;
            _notifs = notifs;
            _context = context;
        }

        public async Task<int> RegisterAsync(RegisterDTO dto, string rol = "Cliente")
        {
            var existing = await _usuarios.FindByCorreoAsync(dto.Correo);
            if (existing != null) throw new InvalidOperationException("El correo ya está registrado.");

            var maxSucursal = await _usuarios.GetMaxNumeroSucursalAsync();
            var nuevoSucursal = maxSucursal + 1;

            var usuario = new Usuarios
            {
                Nombre = dto.Nombre,
                NumeroSucursal = nuevoSucursal,
                Correo = dto.Correo,
                Contrasena = PasswordHasher.Hash(dto.Contrasena),
                FechaCreacion = DateTime.Now
            };

            await _usuarios.CreateAsync(usuario);

            await _notifs.CrearEventoTicketAsync(
                new[] { usuario.IdUsuario },
                0,
                "Registro",
                $"Usuario registrado: {usuario.Nombre}",
                "Sistema");

            return usuario.IdUsuario;
        }

        public async Task<(bool ok, int userId, string nombre, string rol, string error)> LoginAsync(LoginDTO dto, string ipForAudit)
        {
            var user = await _usuarios.FindByCorreoAsync(dto.Correo);
            if (user == null) return (false, 0, "", "", "Credenciales inválidas");

            bool credOk;
            if (user.Contrasena.StartsWith("PBKDF2$"))
            {
                credOk = PasswordHasher.Verify(dto.Contrasena, user.Contrasena);
            }
            else
            {
                credOk = string.Equals(dto.Contrasena, user.Contrasena, StringComparison.Ordinal);
                if (credOk)
                {
                    user.Contrasena = PasswordHasher.Hash(dto.Contrasena);
                }
            }

            if (!credOk) return (false, 0, "", "", "Credenciales inválidas");

            user.UltimoInicioSesion = DateTime.Now;
            await _usuarios.UpdateAsync(user);

            // ✅ OBTENER EL ROL CORRECTAMENTE
            var rol = "Cliente"; // Valor por defecto

            if (user.UsuarioRoles != null && user.UsuarioRoles.Any())
            {
                var primerRol = user.UsuarioRoles.FirstOrDefault();
                if (primerRol?.Rol != null)
                {
                    rol = primerRol.Rol.NombreRol ?? "Cliente";
                }
            }

            // Notificación de login
            await _notifs.CrearLoginAsync(user.IdUsuario);

            return (true, user.IdUsuario, user.Nombre, rol, "");
        }

        public async Task LogoutAsync(int usuarioId)
        {
            await _notifs.CrearLogoutAsync(usuarioId);
        }

        public async Task<ProfileDTO?> GetProfileAsync(int idUsuario)
        {
            var user = await _usuarios.FindByIdAsync(idUsuario);
            if (user == null) return null;
            return new ProfileDTO
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                NumeroSucursal = user.NumeroSucursal,
                Correo = user.Correo
            };
        }

        public async Task<bool> UpdateProfileAsync(ProfileDTO dto)
        {
            var user = await _usuarios.FindByIdAsync(dto.IdUsuario);
            if (user == null) return false;

            if (!string.Equals(user.Correo, dto.Correo, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _usuarios.FindByCorreoAsync(dto.Correo);
                if (exists != null && exists.IdUsuario != user.IdUsuario)
                    throw new InvalidOperationException("El correo ya está en uso.");
                user.Correo = dto.Correo;
            }

            user.Nombre = dto.Nombre;
            user.NumeroSucursal = dto.NumeroSucursal;
            await _usuarios.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int idUsuario, ChangePasswordDTO dto)
        {
            var user = await _usuarios.FindByIdAsync(idUsuario);
            if (user == null) return false;

            if (!PasswordHasher.Verify(dto.ContrasenaActual, user.Contrasena))
                throw new InvalidOperationException("La contraseña actual no es correcta.");

            user.Contrasena = PasswordHasher.Hash(dto.NuevaContrasena);
            await _usuarios.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        private async Task AsignarRolAsync(int idUsuario, string nombreRol)
        {
            // Buscar el rol (case-insensitive)
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol.ToLower() == nombreRol.ToLower());

            if (rol == null)
            {
                // Si no existe el rol, crearlo
                rol = new Roles
                {
                    NombreRol = nombreRol,
                    Descripcion = $"Rol de {nombreRol}"
                };
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();
            }

            // Verificar si ya tiene el rol asignado
            var yaAsignado = await _context.UsuarioRoles
                .AnyAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == rol.IdRol);

            if (!yaAsignado)
            {
                // Asignar el rol
                _context.UsuarioRoles.Add(new UsuarioRol
                {
                    IdUsuario = idUsuario,
                    IdRol = rol.IdRol,
                    FechaAsignacion = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }


        public async Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario)
        {
            var user = await _usuarios.FindByIdAsync(idUsuario);
            if (user == null)
                throw new KeyNotFoundException($"Usuario con ID {idUsuario} no encontrado");

            // Rol por defecto
            var rol = "Cliente";
            var ur = user.UsuarioRoles?.FirstOrDefault();
            if (ur?.Rol != null && !string.IsNullOrWhiteSpace(ur.Rol.NombreRol))
                rol = ur.Rol.NombreRol;

            return new UsuarioDTO
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                Correo = user.Correo,
                Rol = rol
            };
        }

        public async Task<UsuarioDTO?> GetUsuarioInfoAsync(string correo)
        {
            var user = await _usuarios.FindByCorreoAsync(correo);
            if (user == null) return null;
            return new UsuarioDTO { IdUsuario = user.IdUsuario, Nombre = user.Nombre, Correo = user.Correo, Rol = "Cliente" };
        }

        public async Task<bool> ChangePasswordByEmailAsync(string correo, string ultimaContrasena, string nuevaContrasena)
        {
            var user = await _usuarios.FindByCorreoAsync(correo);
            if (user == null) throw new InvalidOperationException("El correo no está registrado.");

            // Validar última contraseña (hash PBKDF2 o legado)
            bool ok;
            if (user.Contrasena.StartsWith("PBKDF2$"))
                ok = PasswordHasher.Verify(ultimaContrasena, user.Contrasena);
            else
                ok = string.Equals(ultimaContrasena, user.Contrasena, StringComparison.Ordinal);

            if (!ok) throw new InvalidOperationException("La última contraseña no es correcta.");

            user.Contrasena = PasswordHasher.Hash(nuevaContrasena);
            await _usuarios.UpdateAsync(user);
            return true;
        }
    }
}
