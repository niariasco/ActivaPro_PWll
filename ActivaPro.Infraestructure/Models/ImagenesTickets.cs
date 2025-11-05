using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class ImagenesTickets
{
    public int IdImagen { get; set; }

    public int IdTicket { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public DateTime? FechaSubida { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;
}
