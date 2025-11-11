using System.ComponentModel.DataAnnotations.Schema;

namespace ActivaPro.Infraestructure.Models
{
    [Table("Categoria_Etiqueta")]
    public class Categoria_Etiqueta
    {
        public int id_categoria { get; set; }
        public int id_etiqueta { get; set; }

        public virtual Categorias Categoria { get; set; } = null!;
        public virtual Etiquetas Etiqueta { get; set; } = null!;
    }
}
