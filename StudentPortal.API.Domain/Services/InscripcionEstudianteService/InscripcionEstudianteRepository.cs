using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentPortal.API.Domain.Contracts.InscripcionEstudianteRepository;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.InscripcionEstudianteInDto;
using StudentPortal.API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.API.Domain.Services.InscripcionEstudianteService
{
    public class InscripcionEstudianteRepository : IInscripcionEstudianteRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<InscripcionEstudianteRepository> _logger;

        public InscripcionEstudianteRepository(DBContext context, ILogger<InscripcionEstudianteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<InscripcionEstudianteDto>> ObtenerPorEstudianteAsync(int estudianteId)
        {
            _logger.LogInformation("Obteniendo inscripciones del estudiante ID: {EstudianteId}", estudianteId);

            var inscripciones = await _context.InscripcionesEstudiantes
                .Where(ie => ie.EstudianteId == estudianteId && ie.Activo)
                .Include(ie => ie.Estudiante)
                    .ThenInclude(e => e.Usuario)
                .Include(ie => ie.Materia)
                    .ThenInclude(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .ThenInclude(p => p.Usuario)
                .AsNoTracking()
                .ToListAsync();

            var inscripcionesDto = Mapping.ConvertirLista<InscripcionesEstudiante, InscripcionEstudianteDto>(inscripciones);

            for (int i = 0; i < inscripciones.Count; i++)
            {
                var entidad = inscripciones[i];
                var dto = inscripcionesDto[i];

                dto.EstudianteNombre = $"{entidad.Estudiante.Usuario.Nombre} {entidad.Estudiante.Usuario.Apellido}";
                dto.MateriaCodigoyNombre = $"{entidad.Materia.Codigo} - {entidad.Materia.Nombre}";
                dto.CredMateria = entidad.Materia.Creditos;
                dto.Profesores = entidad.Materia.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                    .ToList() ?? new List<string>();
            }

            return inscripcionesDto.OrderBy(i => i.FechaInscripcion).ToList();
        }

        public async Task<List<InscripcionEstudianteDto>> ObtenerPorMateriaAsync(int materiaId)
        {
            _logger.LogInformation("Obteniendo inscripciones de la materia ID: {MateriaId}", materiaId);

            var inscripciones = await _context.InscripcionesEstudiantes
                .Where(ie => ie.MateriaId == materiaId && ie.Activo)
                .Include(ie => ie.Estudiante)
                    .ThenInclude(e => e.Usuario)
                .Include(ie => ie.Materia)
                .AsNoTracking()
                .ToListAsync();

            var inscripcionesDto = Mapping.ConvertirLista<InscripcionesEstudiante, InscripcionEstudianteDto>(inscripciones);

            for (int i = 0; i < inscripciones.Count; i++)
            {
                var entidad = inscripciones[i];
                var dto = inscripcionesDto[i];

                dto.EstudianteNombre = $"{entidad.Estudiante.Usuario.Nombre} {entidad.Estudiante.Usuario.Apellido}";
                dto.MateriaCodigoyNombre = $"{entidad.Materia.Codigo} - {entidad.Materia.Nombre}";
                dto.CredMateria = entidad.Materia.Creditos;
            }

            return inscripcionesDto.OrderBy(i => i.EstudianteNombre).ToList();
        }

        public async Task<InscripcionEstudianteDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo inscripción con ID: {Id}", id);

            var inscripcion = await _context.InscripcionesEstudiantes
                .Where(ie => ie.Id == id)
                .Include(ie => ie.Estudiante)
                    .ThenInclude(e => e.Usuario)
                .Include(ie => ie.Materia)
                    .ThenInclude(m => m.ProfesorMateria)
                    .ThenInclude(pm => pm.Profesor)
                    .ThenInclude(p => p.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (inscripcion == null)
                return null;

            var dto = Mapping.Convertir<InscripcionesEstudiante, InscripcionEstudianteDto>(inscripcion);

            dto.EstudianteNombre = $"{inscripcion.Estudiante.Usuario.Nombre} {inscripcion.Estudiante.Usuario.Apellido}";
            dto.MateriaCodigoyNombre = $"{inscripcion.Materia.Codigo} - {inscripcion.Materia.Nombre}";
            dto.CredMateria = inscripcion.Materia.Creditos;
            dto.Profesores = inscripcion.Materia.ProfesorMateria?
                .Where(pm => pm.Activo)
                .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                .ToList() ?? new List<string>();

            return dto;
        }

        public async Task<RespuestaDto> InscribirAsync(InscripcionEstudianteDto inscripcionDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation(
                    "Inscribiendo estudiante ID: {EstudianteId} en materia ID: {MateriaId}",
                    inscripcionDto.EstudianteId,
                    inscripcionDto.MateriaId);

                // Validar que no exista ya una inscripción activa
                if (await ExisteInscripcionAsync(inscripcionDto.EstudianteId, inscripcionDto.MateriaId))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Inscripción fallida",
                        "El estudiante ya está inscrito en esta materia");
                }

                // Validar límite de materias (3)
                var materiasInscritas = await ContarMateriasInscritasAsync(inscripcionDto.EstudianteId);
                if (materiasInscritas >= 3)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Inscripción fallida",
                        "El estudiante ya tiene el máximo de 3 materias inscritas");
                }

                // Validar que no esté con el mismo profesor en otra materia
                if (!await ValidarProfesorDiferenteAsync(inscripcionDto.EstudianteId, inscripcionDto.MateriaId))
                {
                    var materia = await _context.Materias.FindAsync(inscripcionDto.MateriaId);
                    return RespuestaDto.ParametrosIncorrectos(
                        "Inscripción fallida",
                        $"El estudiante ya tiene clases con el profesor de {materia?.Nombre} en otra materia");
                }

                // Crear la inscripción
                var inscripcion = new InscripcionesEstudiante
                {
                    EstudianteId = inscripcionDto.EstudianteId,
                    MateriaId = inscripcionDto.MateriaId,
                    FechaInscripcion = DateTime.Now,
                    Estado = "Inscrito",
                    Activo = true
                };

                await _context.InscripcionesEstudiantes.AddAsync(inscripcion);
                await _context.SaveChangesAsync();

                // Cargar información para la respuesta
                var inscripcionCompleta = await ObtenerPorIdAsync(inscripcion.Id);

                return RespuestaDto.Exitoso(
                    "Inscripción exitosa",
                    $"El estudiante ha sido inscrito en {inscripcionCompleta?.MateriaCodigoyNombre}",
                    inscripcionCompleta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inscribir estudiante {EstudianteId} en materia {MateriaId}",
                    inscripcionDto.EstudianteId, inscripcionDto.MateriaId);

                // Capturar errores específicos de los triggers
                if (ex.InnerException?.Message?.Contains("Un estudiante no puede inscribir más de 3 materias") == true)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Inscripción fallida",
                        "El estudiante ya tiene el máximo de 3 materias inscritas");
                }

                if (ex.InnerException?.Message?.Contains("Un estudiante no puede tener clases con el mismo profesor") == true)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Inscripción fallida",
                        "El estudiante ya tiene clases con este profesor en otra materia");
                }

                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> CalificarAsync(int id, decimal calificacion, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Calificando inscripción ID: {Id} con calificación: {Calificacion}", id, calificacion);

                var inscripcion = await _context.InscripcionesEstudiantes
                    .Include(ie => ie.Estudiante).ThenInclude(e => e.Usuario)
                    .Include(ie => ie.Materia)
                    .FirstOrDefaultAsync(ie => ie.Id == id);

                if (inscripcion == null)
                {
                    return RespuestaDto.NoEncontrado("Inscripción");
                }

                if (inscripcion.Estado != "Inscrito")
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Calificación fallida",
                        "Solo se pueden calificar inscripciones activas");
                }

                inscripcion.Calificacion = calificacion;
                inscripcion.Estado = calificacion >= 3.0m ? "Aprobado" : "Reprobado";
                inscripcion.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Calificación registrada",
                    $"El estudiante {inscripcion.Estudiante.Usuario.Nombre} {inscripcion.Estudiante.Usuario.Apellido} " +
                    $"ha sido calificado con {calificacion} en {inscripcion.Materia.Codigo} - {inscripcion.Materia.Nombre}",
                    await ObtenerPorIdAsync(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calificar inscripción {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> CancelarInscripcionAsync(int id, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Cancelando inscripción ID: {Id}", id);

                var inscripcion = await _context.InscripcionesEstudiantes
                    .Include(ie => ie.Estudiante).ThenInclude(e => e.Usuario)
                    .Include(ie => ie.Materia)
                    .FirstOrDefaultAsync(ie => ie.Id == id);

                if (inscripcion == null)
                {
                    return RespuestaDto.NoEncontrado("Inscripción");
                }

                if (inscripcion.Estado != "Inscrito")
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Cancelación fallida",
                        "Solo se pueden cancelar inscripciones activas");
                }

                inscripcion.Estado = "Cancelado";
                inscripcion.Activo = false;
                inscripcion.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Inscripción cancelada",
                    $"La inscripción del estudiante {inscripcion.Estudiante.Usuario.Nombre} {inscripcion.Estudiante.Usuario.Apellido} " +
                    $"en {inscripcion.Materia.Codigo} - {inscripcion.Materia.Nombre} ha sido cancelada",
                    await ObtenerPorIdAsync(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar inscripción {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<bool> ExisteInscripcionAsync(int estudianteId, int materiaId)
        {
            return await _context.InscripcionesEstudiantes
                .AnyAsync(ie => ie.EstudianteId == estudianteId &&
                              ie.MateriaId == materiaId &&
                              ie.Activo &&
                              ie.Estado == "Inscrito");
        }

        public async Task<int> ContarMateriasInscritasAsync(int estudianteId)
        {
            return await _context.InscripcionesEstudiantes
                .CountAsync(ie => ie.EstudianteId == estudianteId &&
                                 ie.Activo &&
                                 ie.Estado == "Inscrito");
        }

        public async Task<bool> ValidarProfesorDiferenteAsync(int estudianteId, int materiaId)
        {
            // Obtener los profesores de la materia que queremos inscribir
            var profesoresNuevaMateria = await _context.ProfesorMaterias
                .Where(pm => pm.MateriaId == materiaId && pm.Activo)
                .Select(pm => pm.ProfesorId)
                .ToListAsync();

            // Obtener los profesores de las materias en las que el estudiante ya está inscrito
            var profesoresMateriasActuales = await _context.InscripcionesEstudiantes
                .Where(ie => ie.EstudianteId == estudianteId &&
                           ie.Activo &&
                           ie.Estado == "Inscrito")
                .SelectMany(ie => ie.Materia.ProfesorMateria
                    .Where(pm => pm.Activo)
                    .Select(pm => pm.ProfesorId))
                .ToListAsync();

            // Verificar si hay algún profesor en común
            return !profesoresNuevaMateria.Any(p => profesoresMateriasActuales.Contains(p));
        }

        public async Task<PaginacionDto<InscripcionEstudianteDto>> ObtenerPaginadoAsync(
            int pagina,
            int elementosPorPagina,
            int? estudianteId = null,
            int? materiaId = null)
        {
            _logger.LogInformation(
                "Obteniendo inscripciones paginadas. Página: {Pagina}, Elementos: {Elementos}, Estudiante: {EstudianteId}, Materia: {MateriaId}",
                pagina, elementosPorPagina, estudianteId, materiaId);

            IQueryable<InscripcionesEstudiante> query = _context.InscripcionesEstudiantes
                .Include(ie => ie.Estudiante).ThenInclude(e => e.Usuario)
                .Include(ie => ie.Materia).ThenInclude(m => m.ProfesorMateria).ThenInclude(pm => pm.Profesor).ThenInclude(p => p.Usuario)
                .Where(ie => ie.Activo);

            if (estudianteId.HasValue)
            {
                query = query.Where(ie => ie.EstudianteId == estudianteId.Value);
            }

            if (materiaId.HasValue)
            {
                query = query.Where(ie => ie.MateriaId == materiaId.Value);
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            List<InscripcionesEstudiante> inscripciones = await query
                .OrderByDescending(ie => ie.FechaInscripcion)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            List<InscripcionEstudianteDto> inscripcionesDto = Mapping.ConvertirLista<InscripcionesEstudiante, InscripcionEstudianteDto>(inscripciones);

            for (int i = 0; i < inscripciones.Count; i++)
            {
                var entidad = inscripciones[i];
                var dto = inscripcionesDto[i];

                dto.EstudianteNombre = $"{entidad.Estudiante.Usuario.Nombre} {entidad.Estudiante.Usuario.Apellido}";
                dto.MateriaCodigoyNombre = $"{entidad.Materia.Codigo} - {entidad.Materia.Nombre}";
                dto.CredMateria = entidad.Materia.Creditos;
                dto.Profesores = entidad.Materia.ProfesorMateria?
                    .Where(pm => pm.Activo)
                    .Select(pm => $"{pm.Profesor.Usuario.Nombre} {pm.Profesor.Usuario.Apellido}")
                    .ToList() ?? new List<string>();
            }

            return new PaginacionDto<InscripcionEstudianteDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = inscripcionesDto
            };

        }
    }
}
