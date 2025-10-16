using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

public class SLA_Tickets
{
    [Key]
    public int id_sla { get; set; }
    public string descripcion { get; set; }
    public string prioridad { get; set; }

    [ForeignKey("id_categoria")]
    public int id_categoria { get; set; }
    public virtual Categorias Categoria { get; set; }

}
