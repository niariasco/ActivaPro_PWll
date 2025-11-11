using ActivaPro.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivaPro.Infraestructure.Repository.Interfaces
{
    public interface IRepoCategorias
    {
        Task<ICollection<Categorias>> ListAsync();
        Task<Categorias> FindByIdAsync(int id);
        Task<Categorias> FindCategoriaByEtiquetaAsync(int idEtiqueta);  
        Task UpdateAsync(Categorias categoria);
        Task CreateAsync(Categorias categoria);
        Task DeleteAsync(int id);
    }
}