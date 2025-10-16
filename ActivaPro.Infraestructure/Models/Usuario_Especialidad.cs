using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Models
{
    public class Usuario_Especialidad
    {
        [Key]
        public int IdUsuario { get; set; }
        public int IdEspecialidad { get; set; }

        public virtual Usuarios Usuario { get; set; } = null!;
        public virtual Especialidades Especialidad { get; set; } = null!;
    }
}
