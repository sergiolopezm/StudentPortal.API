using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentPortal.API.Domain.Contracts.EstudianteRepository;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.EstudianteInDto;
using StudentPortal.API.Util;

namespace StudentPortal.API.Domain.Services.EstudianteService
{
    public class EstudianteRepository : IEstudianteRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<EstudianteRepository> _logger;

        public EstudianteRepository(DBContext context, ILogger<EstudianteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<EstudianteDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todos los estudiantes");

            var estudiantes = await _context.Estudiantes
                .Where(e => e.Activo)
                .Include(e => e.Usuario)
                .Include(e => e.Programa)
                .Include(e => e.InscripcionesEstudiantes)
                    .ThenInclude(ie => ie.Materia)
                .AsNoTracking()
                .ToListAsync();

            var estudiantesDto = Mapping.ConvertirLista<Estudiante, EstudianteDto>(estudiantes);

            for (int i = 0; i < estudiantes.Count; i++)
            {
                var entidad = estudiantes[i];
                var dto = estudiantesDto[i];

                dto.NombreCompleto = $"{entidad.Usuario.Nombre} {entidad.Usuario.Apellido}";
                dto.Email = entidad.Usuario.Email;
                dto.Programa = entidad.Programa.Nombre;
                dto.MateriasInscritasCount = entidad.InscripcionesEstudiantes?
                    .Count(ie => ie.Activo && ie.Estado == "Inscrito") ?? 0;
                dto.CreditosActuales = entidad.InscripcionesEstudiantes?
                    .Where(ie => ie.Activo && ie.Estado == "Inscrito")
                    .Sum(ie => ie.Materia.Creditos) ?? 0;
            }

            return estudiantesDto.OrderBy(e => e.NombreCompleto).ToList();
        }

        public async Task<EstudianteDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo estudiante con ID: {Id}", id);

            var estudiante = await _context.Estudiantes
                .Where(e => e.Id == id)
                .Include(e => e.Usuario)
                .Include(e => e.Programa)
                .Include(e => e.InscripcionesEstudiantes)
                    .ThenInclude(ie => ie.Materia)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (estudiante == null)
                return null;

            var dto = Mapping.Convertir<Estudiante, EstudianteDto>(estudiante);

            dto.NombreCompleto = $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}";
            dto.Email = estudiante.Usuario.Email;
            dto.Programa = estudiante.Programa.Nombre;
            dto.MateriasInscritasCount = estudiante.InscripcionesEstudiantes?
                .Count(ie => ie.Activo && ie.Estado == "Inscrito") ?? 0;
            dto.CreditosActuales = estudiante.InscripcionesEstudiantes?
                .Where(ie => ie.Activo && ie.Estado == "Inscrito")
                .Sum(ie => ie.Materia.Creditos) ?? 0;

            return dto;
        }

        public async Task<RespuestaDto> CrearAsync(EstudianteDto estudianteDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando estudiante: {Identificacion}", estudianteDto.Identificacion);

                // Validar que no exista un estudiante con la misma identificación
                if (await ExistePorIdentificacionAsync(estudianteDto.Identificacion))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un estudiante con la identificación '{estudianteDto.Identificacion}'");
                }

                // Validar que el usuario exista y tenga el rol de estudiante
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == estudianteDto.UsuarioId);

                if (usuario == null)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario especificado no existe");
                }

                if (usuario.Rol.Nombre != "Estudiante")
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario debe tener el rol de 'Estudiante'");
                }

                // Validar que el usuario no esté asociado a otro estudiante
                if (await _context.Estudiantes.AnyAsync(e => e.UsuarioId == estudianteDto.UsuarioId))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El usuario ya está asociado a otro estudiante");
                }

                // Validar que el programa exista
                var programa = await _context.Programas.FindAsync(estudianteDto.ProgramaId);
                if (programa == null || !programa.Activo)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El programa especificado no existe o no está activo");
                }

                var estudiante = new Estudiante
                {
                    UsuarioId = estudianteDto.UsuarioId,
                    Identificacion = estudianteDto.Identificacion,
                    Telefono = estudianteDto.Telefono,
                    Carrera = estudianteDto.Carrera,
                    ProgramaId = estudianteDto.ProgramaId,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                await _context.Estudiantes.AddAsync(estudiante);
                await _context.SaveChangesAsync();

                // Cargar información para la respuesta
                estudiante = await _context.Estudiantes
                    .Include(e => e.Usuario)
                    .Include(e => e.Programa)
                    .FirstOrDefaultAsync(e => e.Id == estudiante.Id);

                var dto = Mapping.Convertir<Estudiante, EstudianteDto>(estudiante!);
                dto.NombreCompleto = $"{estudiante!.Usuario.Nombre} {estudiante.Usuario.Apellido}";
                dto.Email = estudiante.Usuario.Email;
                dto.Programa = estudiante.Programa.Nombre;
                dto.MateriasInscritasCount = 0;
                dto.CreditosActuales = 0;

                return RespuestaDto.Exitoso(
                    "Estudiante creado",
                    $"El estudiante '{dto.NombreCompleto}' ha sido creado correctamente",
                    dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear estudiante {Identificacion}", estudianteDto.Identificacion);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, EstudianteDto estudianteDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando estudiante con ID: {Id}", id);

                var estudiante = await _context.Estudiantes
                    .Include(e => e.Usuario)
                    .Include(e => e.Programa)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (estudiante == null)
                {
                    return RespuestaDto.NoEncontrado("Estudiante");
                }

                // Validar que no exista otro estudiante con la misma identificación
                if (estudiante.Identificacion != estudianteDto.Identificacion &&
                    await _context.Estudiantes.AnyAsync(e => e.Identificacion == estudianteDto.Identificacion && e.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un estudiante con la identificación '{estudianteDto.Identificacion}'");
                }

                // Validar que el programa exista
                var programa = await _context.Programas.FindAsync(estudianteDto.ProgramaId);
                if (programa == null || !programa.Activo)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El programa especificado no existe o no está activo");
                }

                estudiante.Identificacion = estudianteDto.Identificacion;
                estudiante.Telefono = estudianteDto.Telefono;
                estudiante.Carrera = estudianteDto.Carrera;
                estudiante.ProgramaId = estudianteDto.ProgramaId;
                estudiante.Activo = estudianteDto.Activo;
                estudiante.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                var dto = Mapping.Convertir<Estudiante, EstudianteDto>(estudiante);
                dto.NombreCompleto = $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}";
                dto.Email = estudiante.Usuario.Email;
                dto.Programa = estudiante.Programa.Nombre;

                return RespuestaDto.Exitoso(
                    "Estudiante actualizado",
                    $"El estudiante '{dto.NombreCompleto}' ha sido actualizado correctamente",
                    dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estudiante {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando estudiante con ID: {Id}", id);

                var estudiante = await _context.Estudiantes
                    .Include(e => e.Usuario)
                    .Include(e => e.InscripcionesEstudiantes)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (estudiante == null)
                {
                    return RespuestaDto.NoEncontrado("Estudiante");
                }

                // Verificar si tiene inscripciones activas
                if (estudiante.InscripcionesEstudiantes?.Any(ie => ie.Activo && ie.Estado == "Inscrito") ?? false)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar el estudiante '{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}' porque tiene materias inscritas activas");
                }

                estudiante.Activo = false;
                estudiante.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Estudiante eliminado",
                    $"El estudiante '{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}' ha sido eliminado correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar estudiante {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Estudiantes.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> ExistePorIdentificacionAsync(string identificacion)
        {
            return await _context.Estudiantes.AnyAsync(e => e.Identificacion == identificacion);
        }

        public async Task<PaginacionDto<EstudianteDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo estudiantes paginados. Página: {Pagina}, Elementos: {Elementos}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, busqueda);

            IQueryable<Estudiante> query = _context.Estudiantes
                .Include(e => e.Usuario)
                .Include(e => e.Programa)
                .Include(e => e.InscripcionesEstudiantes)
                    .ThenInclude(ie => ie.Materia);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(e =>
                    e.Identificacion.ToLower().Contains(busqueda) ||
                    e.Usuario.Nombre.ToLower().Contains(busqueda) ||
                    e.Usuario.Apellido.ToLower().Contains(busqueda) ||
                    e.Usuario.Email.ToLower().Contains(busqueda) ||
                    (e.Carrera != null && e.Carrera.ToLower().Contains(busqueda)));
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            List<Estudiante> estudiantes = await query
                .OrderBy(e => e.Usuario.Apellido).ThenBy(e => e.Usuario.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            List<EstudianteDto> estudiantesDto = Mapping.ConvertirLista<Estudiante, EstudianteDto>(estudiantes);

            for (int i = 0; i < estudiantes.Count; i++)
            {
                var entidad = estudiantes[i];
                var dto = estudiantesDto[i];

                dto.NombreCompleto = $"{entidad.Usuario.Nombre} {entidad.Usuario.Apellido}";
                dto.Email = entidad.Usuario.Email;
                dto.Programa = entidad.Programa.Nombre;
                dto.MateriasInscritasCount = entidad.InscripcionesEstudiantes?
                    .Count(ie => ie.Activo && ie.Estado == "Inscrito") ?? 0;
                dto.CreditosActuales = entidad.InscripcionesEstudiantes?
                    .Where(ie => ie.Activo && ie.Estado == "Inscrito")
                    .Sum(ie => ie.Materia.Creditos) ?? 0;
            }

            return new PaginacionDto<EstudianteDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = estudiantesDto
            };
        }

        public async Task<List<CompaneroClaseDto>> ObtenerCompanerosPorEstudianteAsync(int estudianteId)
        {
            _logger.LogInformation("Obteniendo compañeros de clase para estudiante ID: {EstudianteId}", estudianteId);

            var companeros = await _context.VwCompanerosClases
                .Where(v => v.EstudianteId == estudianteId)
                .AsNoTracking()
                .ToListAsync();

            return Mapping.ConvertirLista<VwCompanerosClase, CompaneroClaseDto>(companeros);
        }
    }
}
