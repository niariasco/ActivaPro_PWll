using ActivaPro.Application.DTOs;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Models;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActivaPro.Application.Services.Implementations
{
    public class TecnicoService : ITecnicoService
    {
        private readonly IRepoTecnico _repository;
        private readonly ActivaProContext _context;

        public TecnicoService(ActivaProContext context, IRepoTecnico repository)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<ICollection<TecnicosDTO>> ListAsync()
        {
            var tecnicos = await _repository.ListAsync();
            var result = new List<TecnicosDTO>();

            foreach (var t in tecnicos)
            {
                var join = await _context.Tecnico_Especialidad
                    .Where(te => te.IdTecnico == t.IdTecnico)
                    .Include(te => te.EspecialidadU)
                    .ToListAsync();

                result.Add(new TecnicosDTO
                {
                    IdTecnico = t.IdTecnico,
                    IdUsuario = t.IdUsuario,
                    CargaTrabajo = t.CargaTrabajo,
                    Disponible = t.Disponible,
                    EspecialidadesIds = join.Select(j => j.IdEspecialidadesU).ToList(),
                    EspecialidadesNombres = join.Select(j => j.EspecialidadU.NombreEspecialidadU).ToList(),
                    NombreUsuario = t.Usuario?.Nombre,
                    CorreoUsuario = t.Usuario?.Correo
                });
            }

            return result;
        }

        public async Task<TecnicosDTO?> FindByIdAsync(int id)
        {
            var tecnico = await _repository.FindByIdAsync(id);
            if (tecnico == null) return null;

            var join = await _context.Tecnico_Especialidad
                .Where(te => te.IdTecnico == tecnico.IdTecnico)
                .Include(te => te.EspecialidadU)
                .ToListAsync();

            return new TecnicosDTO
            {
                IdTecnico = tecnico.IdTecnico,
                IdUsuario = tecnico.IdUsuario,
                CargaTrabajo = tecnico.CargaTrabajo,
                Disponible = tecnico.Disponible,
                EspecialidadesIds = join.Select(j => j.IdEspecialidadesU).ToList(),
                EspecialidadesNombres = join.Select(j => j.EspecialidadU.NombreEspecialidadU).ToList(),
                NombreUsuario = tecnico.Usuario?.Nombre,
                CorreoUsuario = tecnico.Usuario?.Correo
            };
        }

        public async Task CreateAsync(TecnicosDTO dto)
        {
            int userId;
            if (dto.IdUsuario > 0)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == dto.IdUsuario);
                if (user == null) throw new InvalidOperationException("El IdUsuario proporcionado no existe.");
                userId = user.IdUsuario;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.CorreoUsuario) || string.IsNullOrWhiteSpace(dto.NombreUsuario))
                    throw new InvalidOperationException("Debe indicar nombre y correo para crear el usuario.");

                var existingByEmail = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == dto.CorreoUsuario);
                if (existingByEmail != null)
                {
                    userId = existingByEmail.IdUsuario;
                }
                else
                {
                    var nuevo = new Usuarios
                    {
                        Nombre = dto.NombreUsuario!,
                        Correo = dto.CorreoUsuario!,
                        NumeroSucursal = 0,
                        Contrasena = Guid.NewGuid().ToString("N")
                    };
                    _context.Usuarios.Add(nuevo);
                    await _context.SaveChangesAsync();
                    userId = nuevo.IdUsuario;
                }
            }

            var entity = new Tecnicos
            {
                IdUsuario = userId,
                Disponible = dto.Disponible,
                CargaTrabajo = 0
            };

            await _repository.CreateAsync(entity);
            await ReplaceTecnicoEspecialidades(entity.IdTecnico, dto.EspecialidadesIds);
        }

        public async Task UpdateAsync(TecnicosDTO dto)
        {
            var entity = await _repository.FindByIdAsync(dto.IdTecnico);
            if (entity == null) throw new KeyNotFoundException("Técnico no encontrado.");

            // Actualiza datos básicos del técnico
            entity.Disponible = dto.Disponible;

            // Actualiza datos del usuario asociado (nombre/correo)
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == entity.IdUsuario);
            if (user == null) throw new KeyNotFoundException("Usuario asociado no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.NombreUsuario))
                user.Nombre = dto.NombreUsuario!;
            if (!string.IsNullOrWhiteSpace(dto.CorreoUsuario))
                user.Correo = dto.CorreoUsuario!;

            // Guardar cambios del técnico (y usuario porque se usa el mismo DbContext)
            await _repository.UpdateAsync(entity);

            // Reemplazar especialidades seleccionadas
            await ReplaceTecnicoEspecialidades(entity.IdTecnico, dto.EspecialidadesIds);
        }

        public async Task<List<(int Id, string Nombre)>> GetEspecialidadesUCatalogAsync()
        {
            return await _context.EspecialidadesU
                .Select(e => new ValueTuple<int, string>(e.IdEspecialidadesU, e.NombreEspecialidadU))
                .ToListAsync();
        }

        private async Task ReplaceTecnicoEspecialidades(int idTecnico, List<int> nuevasIds)
        {
            var actuales = await _context.Tecnico_Especialidad
                .Where(te => te.IdTecnico == idTecnico)
                .ToListAsync();

            _context.Tecnico_Especialidad.RemoveRange(actuales);

            if (nuevasIds != null && nuevasIds.Any())
            {
                foreach (var idEsp in nuevasIds.Distinct())
                {
                    _context.Tecnico_Especialidad.Add(new Tecnico_Especialidad
                    {
                        IdTecnico = idTecnico,
                        IdEspecialidadesU = idEsp
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}