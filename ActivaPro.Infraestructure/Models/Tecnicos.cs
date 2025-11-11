using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models
{
    public partial class Tecnicos
    {
        [Key]
        public int IdTecnico { get; set; }  // Identity en SQL Server, se genera automáticamente
        public int IdUsuario { get; set; }  // FK hacia Usuario
        public int CargaTrabajo { get; set; } = 0;
        public bool Disponible { get; set; } = true;
        public string? Especialidades { get; set; }
        public string? Especialidades { get; set; }
        public string? Especialidades { get; set; }
        public string? Especialidades { get; set; }

    public int IdUsuario { get; set; }

    public int CargaTrabajo { get; set; }

    public bool Disponible { get; set; }

    public string? Especialidades { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
