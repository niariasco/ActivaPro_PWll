using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class ValoracionTickets
{
    [Key]
    public int IdValoracion { get; set; }

    public int IdTicket { get; set; }

    public byte Puntaje { get; set; }

    public string Comentario { get; set; } = null!;

    public DateTime? FechaValoracion { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;
}
