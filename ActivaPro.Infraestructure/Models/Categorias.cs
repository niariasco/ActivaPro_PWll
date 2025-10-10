using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Categorias
{
    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<ReglasAutotriage> ReglasAutotriage { get; set; } = new List<ReglasAutotriage>();

    public virtual ICollection<SlaTickets> SlaTickets { get; set; } = new List<SlaTickets>();

    public virtual ICollection<Tickets> Tickets { get; set; } = new List<Tickets>();
}
