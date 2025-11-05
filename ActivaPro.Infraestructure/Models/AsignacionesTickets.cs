using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class AsignacionesTickets
{
    public int IdAsignacion { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuarioAsignado { get; set; }

    public int? IdUsuarioAsignador { get; set; }

    public string TipoAsignacion { get; set; } = null!;

    public DateTime? FechaAsignacion { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioAsignadoNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioAsignadorNavigation { get; set; }
}
