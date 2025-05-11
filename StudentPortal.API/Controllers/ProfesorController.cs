using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Domain.Contracts.ProfesorRepository;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProfesorInDto;

namespace StudentPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    [ServiceFilter(typeof(LogAttribute))]
    [ServiceFilter(typeof(ExceptionAttribute))]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class ProfesorController : ControllerBase
    {
        private readonly IProfesorRepository _profesorRepository;
        private readonly ILogRepository _logRepository;
        private readonly Domain.Contracts.ILoggerFactory _loggerFactory;

        public ProfesorController(
            IProfesorRepository profesorRepository,
            ILogRepository logRepository,
            Domain.Contracts.ILoggerFactory loggerFactory)
        {
            _profesorRepository = profesorRepository;
            _logRepository = logRepository;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Obtiene todos los profesores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerTodosProfesores");

            try
            {
                await logger.InfoAsync("Iniciando búsqueda de todos los profesores");

                var profesores = await _profesorRepository.ObtenerTodosAsync();

                await logger.InfoAsync($"Se encontraron {profesores.Count} profesores", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Profesores obtenidos",
                    $"Se han obtenido {profesores.Count} profesores",
                    profesores));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodosProfesores",
                    ex.Message);

                await logger.ErrorAsync("Error al obtener todos los profesores", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene un profesor por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ObtenerProfesorPorId-{id}");

            try
            {
                await logger.InfoAsync($"Buscando profesor con ID: {id}");

                var profesor = await _profesorRepository.ObtenerPorIdAsync(id);

                if (profesor == null)
                {
                    await logger.WarningAsync($"Profesor no encontrado con ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Profesor"));
                }

                await logger.InfoAsync($"Profesor encontrado: {profesor.NombreCompleto}", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Profesor obtenido",
                    $"Se ha obtenido el profesor '{profesor.NombreCompleto}'",
                    profesor));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerProfesorPorId: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al obtener profesor con ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de profesores
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerProfesoresPaginado");

            try
            {
                await logger.InfoAsync($"Búsqueda paginada - Página: {pagina}, Elementos: {elementosPorPagina}, Búsqueda: '{busqueda}'");

                var profesores = await _profesorRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, busqueda);

                await logger.InfoAsync($"Resultado paginado - {profesores.Lista?.Count ?? 0} profesores de {profesores.TotalRegistros} total", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Profesores obtenidos",
                    $"Se han obtenido {profesores.Lista?.Count ?? 0} profesores de un total de {profesores.TotalRegistros}",
                    profesores));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerProfesoresPaginado",
                    ex.Message);

                await logger.ErrorAsync("Error en búsqueda paginada de profesores", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea un nuevo profesor
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Crear([FromBody] ProfesorDto profesorDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "CrearProfesor");

            try
            {
                await logger.InfoAsync($"Iniciando creación de profesor - Identificación: {profesorDto.Identificacion}");

                var resultado = await _profesorRepository.CrearAsync(profesorDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearProfesor",
                        $"Se ha creado el profesor con identificación '{profesorDto.Identificacion}'");

                    await logger.ActionAsync($"Profesor creado exitosamente - ID: {profesorDto.Identificacion}");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((ProfesorDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en creación de profesor: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "CrearProfesor",
                    ex.Message);

                await logger.ErrorAsync($"Error al crear profesor - Identificación: {profesorDto.Identificacion}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza un profesor existente
        /// </summary>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProfesorDto profesorDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ActualizarProfesor-{id}");

            try
            {
                if (profesorDto.Id != 0 && profesorDto.Id != id)
                {
                    await logger.WarningAsync($"Discrepancia de ID - URL: {id}, DTO: {profesorDto.Id}");
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID del profesor no coincide con el ID de la URL"));
                }

                await logger.InfoAsync($"Iniciando actualización de profesor - ID: {id}, Identificación: {profesorDto.Identificacion}");

                var existe = await _profesorRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Profesor no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Profesor"));
                }

                var resultado = await _profesorRepository.ActualizarAsync(id, profesorDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarProfesor",
                        $"Se ha actualizado el profesor con identificación '{profesorDto.Identificacion}'");

                    await logger.ActionAsync($"Profesor actualizado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en actualización de profesor: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ActualizarProfesor: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al actualizar profesor - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina un profesor
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"EliminarProfesor-{id}");

            try
            {
                await logger.InfoAsync($"Iniciando eliminación de profesor - ID: {id}");

                var existe = await _profesorRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Profesor no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Profesor"));
                }

                var resultado = await _profesorRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarProfesor",
                        $"Se ha eliminado el profesor con ID '{id}'");

                    await logger.ActionAsync($"Profesor eliminado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"No se pudo eliminar el profesor: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"EliminarProfesor: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al eliminar profesor - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        private Guid GetUsuarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }
}
