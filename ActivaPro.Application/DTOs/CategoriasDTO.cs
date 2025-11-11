using ActivaPro.Infraestructure.Models;
using System.ComponentModel.DataAnnotations;

namespace ActivaPro.Application.DTOs
{
    public class CategoriasDTO
    {

        public int id_categoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        public string nombre_categoria { get; set; } = string.Empty;

        public List<string> Etiquetas { get; set; } = new();
        public List<string> Especialidades { get; set; } = new();

        public int? id_sla { get; set; }
        public string? SLA { get; set; }

        public int tiempo_respuesta_minutos { get; set; }
        public int tiempo_resolucion_minutos { get; set; }
    }
}