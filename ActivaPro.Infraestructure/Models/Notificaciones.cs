using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class Notificaciones
{
    [Key]
    public int IdNotificacion { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string Mensaje { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public bool? Leido { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<ValoracionNotificaciones> ValoracionNotificaciones { get; set; } = new List<ValoracionNotificaciones>();
}
