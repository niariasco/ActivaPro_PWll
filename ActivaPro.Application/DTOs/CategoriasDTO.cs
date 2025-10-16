using ActivaPro.Infraestructure.Models;

namespace ActivaPro.Application.DTOs
{
    public class CategoriasDTO
    {
        public int id_categoria { get; set; }
        public string nombre_categoria { get; set; }
        public List<string> Etiquetas { get; set; } = new ();
       public List<string> Especialidades { get; set; } = new();
        public List<string> SLA { get; set; } = new();
    }
}
