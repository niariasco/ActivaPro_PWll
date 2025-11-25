using System;
using System.Linq;
using System.Threading.Tasks;
using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using ActivaPro.Application.Security;

namespace ActivaPro.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IRepoUsuarios _usuarios;
        private readonly INotificacionService _notifs;

        public AuthService(IRepoUsuarios usuarios, INotificacionService notifs)
        {
            _usuarios = usuarios;
            _notifs = notifs;
        }

        public async Task<int> RegisterAsync(RegisterDTO dto, string rol = "Cliente")
        {
            var existing = await _usuarios.FindByCorreoAsync(dto.Correo);
            if (existing != null) throw new InvalidOperationException("El correo ya está registrado.");

            var usuario = new Usuarios
            {
                Nombre = dto.Nombre,
                NumeroSucursal = dto.NumeroSucursal,
                Correo = dto.Correo,
                Contrasena = PasswordHasher.Hash(dto.Contrasena),
                FechaCreacion = DateTime.Now
            };

            await _usuarios.CreateAsync(usuario);
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

            var rol = user.UsuarioRoles?.FirstOrDefault()?.Rol?.NombreRol ?? "Cliente";

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
    }
}
