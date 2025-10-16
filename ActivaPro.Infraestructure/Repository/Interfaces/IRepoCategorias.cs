﻿using ActivaPro.Infraestructure.Models;
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
    }
}
