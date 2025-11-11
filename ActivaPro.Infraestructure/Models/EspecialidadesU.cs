using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Models
{
    [Table("EspecialidadesU")]
    public class EspecialidadesU
    {
        [Key]
        public int IdEspecialidadesU { get; set; }
        public string NombreEspecialidadU { get; set; } = string.Empty;
    }
}