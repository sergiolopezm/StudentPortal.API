using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Domain.Contracts.EstudianteRepository;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.EstudianteInDto;

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
    public class EstudianteController : ControllerBase
    {
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly ILogRepository _logRepository;
        private readonly Domain.Contracts.ILoggerFactory _loggerFactory;

        public EstudianteController(
            IEstudianteRepository estudianteRepository,
            ILogRepository logRepository,
            Domain.Contracts.ILoggerFactory loggerFactory)
        {
            _estudianteRepository = estudianteRepository;
            _logRepository = logRepository;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Obtiene todos los estudiantes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerTodosEstudiantes");

            try
            {
                await logger.InfoAsync("Iniciando búsqueda de todos los estudiantes");

                var estudiantes = await _estudianteRepository.ObtenerTodosAsync();

                await logger.InfoAsync($"Se encontraron {estudiantes.Count} estudiantes", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Estudiantes obtenidos",
                    $"Se han obtenido {estudiantes.Count} estudiantes",
                    estudiantes));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodosEstudiantes",
                    ex.Message);

                await logger.ErrorAsync("Error al obtener todos los estudiantes", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene un estudiante por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ObtenerEstudiantePorId-{id}");

            try
            {
                await logger.InfoAsync($"Buscando estudiante con ID: {id}");

                var estudiante = await _estudianteRepository.ObtenerPorIdAsync(id);

                if (estudiante == null)
                {
                    await logger.WarningAsync($"Estudiante no encontrado con ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Estudiante"));
                }

                await logger.InfoAsync($"Estudiante encontrado: {estudiante.NombreCompleto}", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Estudiante obtenido",
                    $"Se ha obtenido el estudiante '{estudiante.NombreCompleto}'",
                    estudiante));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerEstudiantePorId: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al obtener estudiante con ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de estudiantes
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerEstudiantesPaginado");

            try
            {
                await logger.InfoAsync($"Búsqueda paginada - Página: {pagina}, Elementos: {elementosPorPagina}, Búsqueda: '{busqueda}'");

                var estudiantes = await _estudianteRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, busqueda);

                await logger.InfoAsync($"Resultado paginado - {estudiantes.Lista?.Count ?? 0} estudiantes de {estudiantes.TotalRegistros} total", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Estudiantes obtenidos",
                    $"Se han obtenido {estudiantes.Lista?.Count ?? 0} estudiantes de un total de {estudiantes.TotalRegistros}",
                    estudiantes));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerEstudiantesPaginado",
                    ex.Message);

                await logger.ErrorAsync("Error en búsqueda paginada de estudiantes", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea un nuevo estudiante
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Crear([FromBody] EstudianteDto estudianteDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "CrearEstudiante");

            try
            {
                await logger.InfoAsync($"Iniciando creación de estudiante - Identificación: {estudianteDto.Identificacion}");

                var resultado = await _estudianteRepository.CrearAsync(estudianteDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearEstudiante",
                        $"Se ha creado el estudiante con identificación '{estudianteDto.Identificacion}'");

                    await logger.ActionAsync($"Estudiante creado exitosamente - ID: {estudianteDto.Identificacion}");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((EstudianteDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en creación de estudiante: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "CrearEstudiante",
                    ex.Message);

                await logger.ErrorAsync($"Error al crear estudiante - Identificación: {estudianteDto.Identificacion}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza un estudiante existente
        /// </summary>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Actualizar(int id, [FromBody] EstudianteDto estudianteDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ActualizarEstudiante-{id}");

            try
            {
                if (estudianteDto.Id != 0 && estudianteDto.Id != id)
                {
                    await logger.WarningAsync($"Discrepancia de ID - URL: {id}, DTO: {estudianteDto.Id}");
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID del estudiante no coincide con el ID de la URL"));
                }

                await logger.InfoAsync($"Iniciando actualización de estudiante - ID: {id}, Identificación: {estudianteDto.Identificacion}");

                var existe = await _estudianteRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Estudiante no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Estudiante"));
                }

                var resultado = await _estudianteRepository.ActualizarAsync(id, estudianteDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarEstudiante",
                        $"Se ha actualizado el estudiante con identificación '{estudianteDto.Identificacion}'");

                    await logger.ActionAsync($"Estudiante actualizado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en actualización de estudiante: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ActualizarEstudiante: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al actualizar estudiante - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina un estudiante
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"EliminarEstudiante-{id}");

            try
            {
                await logger.InfoAsync($"Iniciando eliminación de estudiante - ID: {id}");

                var existe = await _estudianteRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Estudiante no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Estudiante"));
                }

                var resultado = await _estudianteRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarEstudiante",
                        $"Se ha eliminado el estudiante con ID '{id}'");

                    await logger.ActionAsync($"Estudiante eliminado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"No se pudo eliminar el estudiante: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"EliminarEstudiante: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al eliminar estudiante - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene los compañeros de clase de un estudiante
        /// </summary>
        [HttpGet("{id}/companeros")]
        public async Task<IActionResult> ObtenerCompaneros(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ObtenerCompanerosEstudiante-{id}");

            try
            {
                await logger.InfoAsync($"Buscando compañeros de clase para estudiante - ID: {id}");

                var existe = await _estudianteRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Estudiante no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Estudiante"));
                }

                var companeros = await _estudianteRepository.ObtenerCompanerosPorEstudianteAsync(id);

                await logger.InfoAsync($"Se encontraron {companeros.Count} compañeros de clase", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Compañeros obtenidos",
                    $"Se han obtenido {companeros.Count} compañeros de clase",
                    companeros));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerCompanerosEstudiante: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al obtener compañeros de estudiante - ID: {id}", ex);

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
