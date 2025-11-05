using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Etiquetas
{
    public int IdEtiqueta { get; set; }

    public string NombreEtiqueta { get; set; } = null!;

    public int? IdCategoria { get; set; }

    public virtual Categorias? IdCategoriaNavigation { get; set; }

    public virtual ICollection<Tickets> IdTicket { get; set; } = new List<Tickets>();
}
