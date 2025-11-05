using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Tickets
{
    public int IdTicket { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int IdUsuarioSolicitante { get; set; }

    public int? IdUsuarioAsignado { get; set; }

    public string Estado { get; set; } = null!;

    public int? IdValoracion { get; set; }

    public int? IdCategoria { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? IdSla { get; set; }

    public DateTime? FechaLimiteResolucion { get; set; }

    public virtual ICollection<AsignacionesTickets> AsignacionesTickets { get; set; } = new List<AsignacionesTickets>();

    public virtual ICollection<HistorialTickets> HistorialTickets { get; set; } = new List<HistorialTickets>();

    public virtual Categorias? IdCategoriaNavigation { get; set; }

    public virtual SlaTickets? IdSlaNavigation { get; set; }

    public virtual Usuarios? IdUsuarioAsignadoNavigation { get; set; }

    public virtual Usuarios IdUsuarioSolicitanteNavigation { get; set; } = null!;

    public virtual ICollection<ImagenesTickets> ImagenesTickets { get; set; } = new List<ImagenesTickets>();

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual ICollection<ValoracionTickets> ValoracionTickets { get; set; } = new List<ValoracionTickets>();

    public virtual ICollection<Etiquetas> IdEtiqueta { get; set; } = new List<Etiquetas>();
}
