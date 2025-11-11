using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Models
{
    public partial class Tecnicos
    {
        [Key]
        public int IdTecnico { get; set; }  // Identity en SQL Server, se genera automáticamente
        public int IdUsuario { get; set; }  // FK hacia Usuario
        public int CargaTrabajo { get; set; } = 0;
        public bool Disponible { get; set; } = true;

        // Información del Usuario relacionada
        [ForeignKey("IdUsuario")]
        public virtual Usuarios Usuario { get; set; }


        //  public virtual ICollection<Usuario_Especialidad> UsuarioEspecialidades { get; set; } = new List<Usuario_Especialidad>();

    }

}
