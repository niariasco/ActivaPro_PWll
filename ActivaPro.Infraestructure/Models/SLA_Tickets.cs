using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("SLA_Tickets")]
    public class SLA_Tickets
    {
        [Key]
        [Column("id_sla")]
        public int id_sla { get; set; }

        [Column("descripcion")]
        public string? descripcion { get; set; }

        [Column("prioridad")]
        public string? prioridad { get; set; }

        [Column("tiempo_resolucion_horas")]
        public int? tiempo_resolucion_horas { get; set; }
    }
}