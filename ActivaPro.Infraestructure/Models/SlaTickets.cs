using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class SlaTickets
{
    public int IdSla { get; set; }

    public int? IdCategoria { get; set; }

    public string Prioridad { get; set; } = null!;

    public int TiempoResolucionHoras { get; set; }

    public string? Descripcion { get; set; }

    public virtual Categorias? IdCategoriaNavigation { get; set; }

    public virtual ICollection<Tickets> Tickets { get; set; } = new List<Tickets>();
}
