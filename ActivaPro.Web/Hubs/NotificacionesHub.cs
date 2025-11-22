using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ActivaPro.Web.Hubs
{
    [Authorize]
    public class NotificacionesHub : Hub
    {
    }
}
