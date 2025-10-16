using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class ImagenesTickets
{
    [Key]
    public int IdImagen { get; set; }

    public int IdTicket { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public DateTime? FechaSubida { get; set; }

    public virtual Tickets IdTicketNavigation { get; set; } = null!;
}
