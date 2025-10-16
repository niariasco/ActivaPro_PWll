using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Infraestructure.Models;

public partial class ValoracionNotificaciones
{
    [Key]
    public int IdValoracion { get; set; }

    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public byte Puntaje { get; set; }

    public string? Comentario { get; set; }

    public DateTime? FechaValoracion { get; set; }

    public virtual Notificaciones IdNotificacionNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
