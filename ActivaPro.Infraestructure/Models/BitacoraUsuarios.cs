using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class BitacoraUsuarios
{
    [Key]
    public int IdBitacora { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime? FechaAccion { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
