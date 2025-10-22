﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models;

[Table("Imagenes_Tickets")]
public class Imagenes_Tickets
{
    [Key]
    [Column("id_imagen")]
    public int IdImagen { get; set; }
    
    [Column("id_ticket")]
    public int IdTicket { get; set; }
    
    [Column("nombre_archivo")]
    public string NombreArchivo { get; set; }
    
    [Column("ruta_archivo")]
    public string RutaArchivo { get; set; }
    
    [Column("fecha_subida")]
    public DateTime FechaSubida { get; set; }
    
    [ForeignKey("IdTicket")]
    public virtual Tickets Ticket { get; set; }
}

[Table("Historial_Tickets")]
public class Historial_Tickets
{
    [Key]
    [Column("id_historial")]
    public int IdHistorial { get; set; }
    
    [Column("id_ticket")]
    public int IdTicket { get; set; }
    
    [Column("id_usuario")]
    public int IdUsuario { get; set; }
    
    [Column("accion")]
    public string Accion { get; set; }
    
    [Column("fecha_accion")]
    public DateTime FechaAccion { get; set; }
    
    [ForeignKey("IdTicket")]
    public virtual Tickets Ticket { get; set; }
    
    [ForeignKey("IdUsuario")]
    public virtual Usuarios Usuario { get; set; }
}

[Table("Valoracion_Tickets")]
public class Valoracion_Tickets
{
    [Key]
    [Column("id_valoracion")]
    public int IdValoracion { get; set; }
    
    [Column("id_ticket")]
    public int IdTicket { get; set; }
    
    [Column("puntaje")]
    public byte Puntaje { get; set; }
    
    [Column("comentario")]
    public string Comentario { get; set; }
    
    [Column("fecha_valoracion")]
    public DateTime FechaValoracion { get; set; }
    
    [ForeignKey("IdTicket")]
    public virtual Tickets Ticket { get; set; }
}