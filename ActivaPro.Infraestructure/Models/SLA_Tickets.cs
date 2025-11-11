using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("SLA_Tickets")]
    public class SLA_Tickets
    {
        [Key]
        public int id_sla { get; set; }

        public string? descripcion { get; set; }
        public string? prioridad { get; set; }

     
    }
}
