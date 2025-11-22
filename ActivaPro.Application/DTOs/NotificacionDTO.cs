using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Application.DTOs
{
    public class NotificacionDTO
    {
        public int IdNotificacion { get; set; }          // int para alinearse con BD
        public int? IdTicket { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leido { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
