using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentPortal.API.Domain.Contracts.ProfesorRepository;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProfesorInDto;
using StudentPortal.API.Util;

namespace StudentPortal.API.Domain.Services.ProfesorService
{
    public class ProfesorRepository : IProfesorRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<ProfesorRepository> _logger;

        public ProfesorRepository(DBContext context, ILogger<ProfesorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProfesorDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todos los profesores");

            var profesores = await _context.Profesores
                .Where(p => p.Activo)
                .Include(p => p.Usuario)
                .Include(p => p.ProfesorMateria)
                    .ThenInclude(pm => pm.Materia)
                .AsNoTracking()
                .ToListAsync();

            var profesoresDto = Mapping.ConvertirLista<Profesore, ProfesorDto>(profesores);

            for (int i = 0; i < profesores.Count; i++)
            {
                var entidad = profesores[i];
                var dto = profesoresDto[i];

                dto.NombreCompleto = $"{entidad.Usuario.Nombre} {entidad.Usuario.Apellido}";
                dto.Email = entidad.Usuario.Email;
                dto.MateriasCount = entidad.ProfesorMateria?.Count(pm => pm.Activo) ?? 0;
                dto.Materias = entidad.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Materia.Codigo} - {pm.Materia.Nombre}")
                    .ToList() ?? new List<string>();
            }

            return profesoresDto.OrderBy(p => p.NombreCompleto).ToList();
        }

        public async Task<ProfesorDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo profesor con ID: {Id}", id);

            var profesor = await _context.Profesores
                .Where(p => p.Id == id)
                .Include(p => p.Usuario)
                .Include(p => p.ProfesorMateria)
                    .ThenInclude(pm => pm.Materia)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profesor == null)
                return null;

            var dto = Mapping.Convertir<Profesore, ProfesorDto>(profesor);

            dto.NombreCompleto = $"{profesor.Usuario.Nombre} {profesor.Usuario.Apellido}";
            dto.Email = profesor.Usuario.Email;
            dto.MateriasCount = profesor.ProfesorMateria?.Count(pm => pm.Activo) ?? 0;
            dto.Materias = profesor.ProfesorMateria?
                .Where(pm => pm.Activo)
                .Select(pm => $"{pm.Materia.Codigo} - {pm.Materia.Nombre}")
                .ToList() ?? new List<string>();

            return dto;
        }

        public async Task<RespuestaDto> CrearAsync(ProfesorDto profesorDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando profesor: {Identificacion}", profesorDto.Identificacion);

                // Validar que no exista un profesor con la misma identificación
                if (await ExistePorIdentificacionAsync(profesorDto.Identificacion))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un profesor con la identificación '{profesorDto.Identificacion}'");
                }

                // Validar que el usuario exista y tenga el rol de profesor
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == profesorDto.UsuarioId);

                if (usuario == null)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario especificado no existe");
                }

                if (usuario.Rol.Nombre != "Profesor")
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario debe tener el rol de 'Profesor'");
                }

                // Validar que el usuario no esté asociado a otro profesor
                if (await _context.Profesores.AnyAsync(p => p.UsuarioId == profesorDto.UsuarioId))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario ya está asociado a otro profesor");
                }

                var profesor = new Profesore
                {
                    UsuarioId = profesorDto.UsuarioId,
                    Identificacion = profesorDto.Identificacion,
                    Telefono = profesorDto.Telefono,
                    Departamento = profesorDto.Departamento,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                await _context.Profesores.AddAsync(profesor);
                await _context.SaveChangesAsync();

                // Cargar información para la respuesta
                profesor = await _context.Profesores
                    .Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.Id == profesor.Id);

                var dto = Mapping.Convertir<Profesore, ProfesorDto>(profesor!);
                dto.NombreCompleto = $"{profesor!.Usuario.Nombre} {profesor.Usuario.Apellido}";
                dto.Email = profesor.Usuario.Email;
                dto.MateriasCount = 0;
                dto.Materias = new List<string>();

                return RespuestaDto.Exitoso(
                    "Profesor creado",
                    $"El profesor '{dto.NombreCompleto}' ha sido creado correctamente",
                    dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear profesor {Identificacion}", profesorDto.Identificacion);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, ProfesorDto profesorDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando profesor con ID: {Id}", id);

                var profesor = await _context.Profesores
                    .Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (profesor == null)
                {
                    return RespuestaDto.NoEncontrado("Profesor");
                }

                // Validar que no exista otro profesor con la misma identificación
                if (profesor.Identificacion != profesorDto.Identificacion &&
                    await _context.Profesores.AnyAsync(p => p.Identificacion == profesorDto.Identificacion && p.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un profesor con la identificación '{profesorDto.Identificacion}'");
                }

                profesor.Identificacion = profesorDto.Identificacion;
                profesor.Telefono = profesorDto.Telefono;
                profesor.Departamento = profesorDto.Departamento;
                profesor.Activo = profesorDto.Activo;
                profesor.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                var dto = Mapping.Convertir<Profesore, ProfesorDto>(profesor);
                dto.NombreCompleto = $"{profesor.Usuario.Nombre} {profesor.Usuario.Apellido}";
                dto.Email = profesor.Usuario.Email;

                return RespuestaDto.Exitoso(
                    "Profesor actualizado",
                    $"El profesor '{dto.NombreCompleto}' ha sido actualizado correctamente",
                    dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar profesor {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando profesor con ID: {Id}", id);

                var profesor = await _context.Profesores
                    .Include(p => p.Usuario)
                    .Include(p => p.ProfesorMateria)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (profesor == null)
                {
                    return RespuestaDto.NoEncontrado("Profesor");
                }

                // Verificar si tiene materias asociadas activas
                if (profesor.ProfesorMateria?.Any(pm => pm.Activo) ?? false)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar el profesor '{profesor.Usuario.Nombre} {profesor.Usuario.Apellido}' porque tiene materias asignadas");
                }

                profesor.Activo = false;
                profesor.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Profesor eliminado",
                    $"El profesor '{profesor.Usuario.Nombre} {profesor.Usuario.Apellido}' ha sido eliminado correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar profesor {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Profesores.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistePorIdentificacionAsync(string identificacion)
        {
            return await _context.Profesores.AnyAsync(p => p.Identificacion == identificacion);
        }

        public async Task<PaginacionDto<ProfesorDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo profesores paginados. Página: {Pagina}, Elementos: {Elementos}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, busqueda);

            IQueryable<Profesore> query = _context.Profesores
                .Include(p => p.Usuario)
                .Include(p => p.ProfesorMateria)
                    .ThenInclude(pm => pm.Materia);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(p =>
                    p.Identificacion.ToLower().Contains(busqueda) ||
                    p.Usuario.Nombre.ToLower().Contains(busqueda) ||
                    p.Usuario.Apellido.ToLower().Contains(busqueda) ||
                    p.Usuario.Email.ToLower().Contains(busqueda) ||
                    (p.Departamento != null && p.Departamento.ToLower().Contains(busqueda)));
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            List<Profesore> profesores = await query
                .OrderBy(p => p.Usuario.Apellido).ThenBy(p => p.Usuario.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            List<ProfesorDto> profesoresDto = Mapping.ConvertirLista<Profesore, ProfesorDto>(profesores);

            for (int i = 0; i < profesores.Count; i++)
            {
                var entidad = profesores[i];
                var dto = profesoresDto[i];

                dto.NombreCompleto = $"{entidad.Usuario.Nombre} {entidad.Usuario.Apellido}";
                dto.Email = entidad.Usuario.Email;
                dto.MateriasCount = entidad.ProfesorMateria?.Count(pm => pm.Activo) ?? 0;
                dto.Materias = entidad.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Materia.Codigo} - {pm.Materia.Nombre}")
                    .ToList() ?? new List<string>();
            }

            return new PaginacionDto<ProfesorDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = profesoresDto
            };
        }
    }
}
