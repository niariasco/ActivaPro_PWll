using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Categoria_Especialidad")]
    public class Categoria_Especialidad
    {
        public int id_categoria { get; set; }
        public int id_especialidad { get; set; }

        [ForeignKey(nameof(id_categoria))]
        public virtual Categorias Categoria { get; set; } = null!;

        [ForeignKey(nameof(id_especialidad))]
        public virtual Especialidades Especialidad { get; set; } = null!;
    }
}
