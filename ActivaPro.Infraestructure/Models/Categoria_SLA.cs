using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Categoria_SLA")]
    public class Categoria_SLA
    {
        public int id_categoria { get; set; }
        public int id_sla { get; set; }

        [ForeignKey(nameof(id_categoria))]
        public virtual Categorias Categoria { get; set; } = null!;

        [ForeignKey(nameof(id_sla))]
        public virtual SLA_Tickets SLA { get; set; } = null!;
    }
}
