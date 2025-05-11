using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentPortal.API.Domain.Contracts.MateriaRepository;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.MateriaInDto;
using StudentPortal.API.Util;

namespace StudentPortal.API.Domain.Services.MateriaService
{
    public class MateriaRepository : IMateriaRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<MateriaRepository> _logger;

        public MateriaRepository(DBContext context, ILogger<MateriaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MateriaDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todas las materias");

            var materias = await _context.Materias
                .Where(m => m.Activo)
                .Include(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .ThenInclude(p => p.Usuario)
                .Include(m => m.InscripcionesEstudiantes)
                .Include(m => m.CreadoPor)
                .Include(m => m.ModificadoPor)
                .AsNoTracking()
                .ToListAsync();

            var materiasDto = Mapping.ConvertirLista<Materia, MateriaDto>(materias);

            for (int i = 0; i < materias.Count; i++)
            {
                var entidad = materias[i];
                var dto = materiasDto[i];

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;

                dto.InscripcionesCount = entidad.InscripcionesEstudiantes?.Count(ie => ie.Activo) ?? 0;

                dto.Profesores = entidad.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                    .ToList() ?? new List<string>();
            }

            return materiasDto.OrderBy(m => m.Codigo).ToList();
        }

        public async Task<MateriaDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo materia con ID: {Id}", id);

            var materia = await _context.Materias
                .Where(m => m.Id == id)
                .Include(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .ThenInclude(p => p.Usuario)
                .Include(m => m.InscripcionesEstudiantes)
                .Include(m => m.CreadoPor)
                .Include(m => m.ModificadoPor)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (materia == null)
                return null;

            var dto = Mapping.Convertir<Materia, MateriaDto>(materia);

            dto.CreadoPor = materia.CreadoPor != null
                ? $"{materia.CreadoPor.Nombre} {materia.CreadoPor.Apellido}"
                : null;

            dto.ModificadoPor = materia.ModificadoPor != null
                ? $"{materia.ModificadoPor.Nombre} {materia.ModificadoPor.Apellido}"
                : null;

            dto.InscripcionesCount = materia.InscripcionesEstudiantes?.Count(ie => ie.Activo) ?? 0;

            dto.Profesores = materia.ProfesorMateria?
                .Where(pm => pm.Activo)
                .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                .ToList() ?? new List<string>();

            return dto;
        }

        public async Task<RespuestaDto> CrearAsync(MateriaDto materiaDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando materia: {Codigo} - {Nombre}", materiaDto.Codigo, materiaDto.Nombre);

                // Validar que no exista una materia con el mismo código
                if (await ExistePorCodigoAsync(materiaDto.Codigo))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe una materia con el código '{materiaDto.Codigo}'");
                }

                var materia = new Materia
                {
                    Codigo = materiaDto.Codigo.ToUpper(),
                    Nombre = materiaDto.Nombre,
                    Descripcion = materiaDto.Descripcion,
                    Creditos = materiaDto.Creditos,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    CreadoPorId = usuarioId
                };

                await _context.Materias.AddAsync(materia);
                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Materia creada",
                    $"La materia '{materia.Codigo} - {materia.Nombre}' ha sido creada correctamente",
                    Mapping.Convertir<Materia, MateriaDto>(materia));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear materia {Codigo}", materiaDto.Codigo);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, MateriaDto materiaDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando materia con ID: {Id}", id);

                var materia = await _context.Materias.FindAsync(id);
                if (materia == null)
                {
                    return RespuestaDto.NoEncontrado("Materia");
                }

                // Validar que no exista otra materia con el mismo código
                if (materia.Codigo != materiaDto.Codigo &&
                    await _context.Materias.AnyAsync(m => m.Codigo == materiaDto.Codigo && m.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe una materia con el código '{materiaDto.Codigo}'");
                }

                materia.Codigo = materiaDto.Codigo.ToUpper();
                materia.Nombre = materiaDto.Nombre;
                materia.Descripcion = materiaDto.Descripcion;
                materia.Creditos = materiaDto.Creditos;
                materia.Activo = materiaDto.Activo;
                materia.FechaModificacion = DateTime.Now;
                materia.ModificadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Materia actualizada",
                    $"La materia '{materia.Codigo} - {materia.Nombre}' ha sido actualizada correctamente",
                    Mapping.Convertir<Materia, MateriaDto>(materia));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar materia {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando materia con ID: {Id}", id);

                var materia = await _context.Materias
                    .Include(m => m.InscripcionesEstudiantes)
                    .Include(m => m.ProfesorMateria)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (materia == null)
                {
                    return RespuestaDto.NoEncontrado("Materia");
                }

                // Verificar si tiene inscripciones activas
                if (materia.InscripcionesEstudiantes?.Any(ie => ie.Activo) ?? false)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar la materia '{materia.Codigo} - {materia.Nombre}' porque tiene estudiantes inscritos");
                }

                // Desactivar la materia y sus relaciones
                materia.Activo = false;
                materia.FechaModificacion = DateTime.Now;

                // Desactivar las relaciones de profesores si existen
                if (materia.ProfesorMateria != null)
                {
                    foreach (var pm in materia.ProfesorMateria)
                    {
                        pm.Activo = false;
                    }
                }

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Materia eliminada",
                    $"La materia '{materia.Codigo} - {materia.Nombre}' ha sido eliminada correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar materia {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Materias.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> ExistePorCodigoAsync(string codigo)
        {
            return await _context.Materias.AnyAsync(m => m.Codigo == codigo.ToUpper());
        }

        public async Task<PaginacionDto<MateriaDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo materias paginadas. Página: {Pagina}, Elementos: {Elementos}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, busqueda);

            IQueryable<Materia> query = _context.Materias
                .Include(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .ThenInclude(p => p.Usuario)
                .Include(m => m.InscripcionesEstudiantes)
                .Include(m => m.CreadoPor)
                .Include(m => m.ModificadoPor);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(m =>
                    m.Codigo.ToLower().Contains(busqueda) ||
                    m.Nombre.ToLower().Contains(busqueda) ||
                    (m.Descripcion != null && m.Descripcion.ToLower().Contains(busqueda)));
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            List<Materia> materias = await query
                .OrderBy(m => m.Codigo)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            List<MateriaDto> materiasDto = Mapping.ConvertirLista<Materia, MateriaDto>(materias);

            for (int i = 0; i < materias.Count; i++)
            {
                var entidad = materias[i];
                var dto = materiasDto[i];

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;

                dto.InscripcionesCount = entidad.InscripcionesEstudiantes?.Count(ie => ie.Activo) ?? 0;

                dto.Profesores = entidad.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                    .ToList() ?? new List<string>();
            }

            return new PaginacionDto<MateriaDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = materiasDto
            };
        }
    }
}
