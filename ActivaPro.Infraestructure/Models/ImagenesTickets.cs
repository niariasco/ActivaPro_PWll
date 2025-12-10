using System;
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