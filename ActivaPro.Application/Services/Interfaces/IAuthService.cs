using ActivaPro.Application.DTOs;
using System.Threading.Tasks;

namespace ActivaPro.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterAsync(RegisterDTO dto, string rol = "Cliente");
        Task<(bool ok, int userId, string nombre, string rol, string error)> LoginAsync(LoginDTO dto, string ipForAudit);
        Task LogoutAsync(int usuarioId);              
        Task<ProfileDTO?> GetProfileAsync(int idUsuario);
        Task<bool> UpdateProfileAsync(ProfileDTO dto);
        Task<bool> ChangePasswordAsync(int idUsuario, ChangePasswordDTO dto);

        Task<bool> ChangePasswordByEmailAsync(string correo, string ultimaContrasena, string nuevaContrasena);

        Task<UsuarioDTO?> GetUsuarioInfoAsync(string correo);
        Task<UsuarioDTO> GetUsuarioInfoAsync(int idUsuario);
    }
}
