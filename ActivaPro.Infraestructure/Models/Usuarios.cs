using System;
using System.Collections.Generic;

namespace ActivaPro.Infraestructure.Models;

public partial class Usuarios
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public int NumeroSucursal { get; set; }

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<AsignacionesTickets> AsignacionesTicketsIdUsuarioAsignadoNavigation { get; set; } = new List<AsignacionesTickets>();

    public virtual ICollection<AsignacionesTickets> AsignacionesTicketsIdUsuarioAsignadorNavigation { get; set; } = new List<AsignacionesTickets>();

    public virtual ICollection<BitacoraUsuarios> BitacoraUsuarios { get; set; } = new List<BitacoraUsuarios>();

    public virtual ICollection<HistorialTickets> HistorialTickets { get; set; } = new List<HistorialTickets>();

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual ICollection<ReglasAutotriage> ReglasAutotriage { get; set; } = new List<ReglasAutotriage>();

    public virtual ICollection<Tickets> TicketsIdUsuarioAsignadoNavigation { get; set; } = new List<Tickets>();

    public virtual ICollection<Tickets> TicketsIdUsuarioSolicitanteNavigation { get; set; } = new List<Tickets>();

    public virtual ICollection<UsuarioRol> UsuarioRol { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<ValoracionNotificaciones> ValoracionNotificaciones { get; set; } = new List<ValoracionNotificaciones>();

    public virtual ICollection<Especialidades> IdEspecialidad { get; set; } = new List<Especialidades>();
}
