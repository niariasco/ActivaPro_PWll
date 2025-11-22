using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ActivaPro.Web.Security
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection) =>
            connection.User?.FindFirst("id_usuario")?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}