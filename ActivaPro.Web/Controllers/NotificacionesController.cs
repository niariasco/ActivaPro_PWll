using ActivaPro.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActivaPro.Web.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly INotificacionService _service;
        public NotificacionesController(INotificacionService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Panel(int skip = 0, int take = 30)
        {
            var uid = GetUserId();
            var list = await _service.ListarAsync(uid, skip, take);
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> Unread()
        {
            var uid = GetUserId();
            var count = await _service.NoLeidasAsync(uid);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var ok = await _service.MarcarLeidaAsync(id, GetUserId());
            var count = await _service.NoLeidasAsync(GetUserId());
            return Json(new { success = ok, unread = count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var uid = GetUserId();
            var changed = await _service.MarcarTodasLeidasAsync(uid);
            var unread = await _service.NoLeidasAsync(uid);
            return Json(new { success = true, changed, unread });
        }

        [HttpGet]
        public IActionResult Historial()
        {
            return View(); // Se carga vía JS para paginación dinámica
        }

        private int GetUserId()
        {
            var c = User.FindFirst("id_usuario") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return c != null ? int.Parse(c.Value) : 0;
        }
    }
}
