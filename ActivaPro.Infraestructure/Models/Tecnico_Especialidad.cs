using System.ComponentModel.DataAnnotations.Schema;
using ActivaPro.Infraestructure.Models;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Tecnico_Especialidad")]
    public class Tecnico_Especialidad
    {
        public int IdTecnico { get; set; }
        public int IdEspecialidadesU { get; set; } // FK a Especialidades.id_especialidad

        [ForeignKey(nameof(IdTecnico))]
        public virtual Tecnicos Tecnico { get; set; } = null!;

        // Apunta a la tabla EspecialidadesU (no a Especialidades)
        [ForeignKey(nameof(IdEspecialidadesU))]
        public virtual EspecialidadesU EspecialidadU { get; set; } = null!;
    }
}