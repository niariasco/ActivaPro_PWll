using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class HistorialTickets
{
    [Key]
    public int IdHistorial { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime? FechaAccion { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
