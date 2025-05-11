using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts.ProgramaRepository;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProgramaInDto;

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
    public class ProgramaController : ControllerBase
    {
        private readonly IProgramaRepository _programaRepository;
        private readonly ILogRepository _logRepository;
        private readonly Domain.Contracts.ILoggerFactory _loggerFactory;

        public ProgramaController(
            IProgramaRepository programaRepository,
            ILogRepository logRepository,
            Domain.Contracts.ILoggerFactory loggerFactory)
        {
            _programaRepository = programaRepository;
            _logRepository = logRepository;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Obtiene todos los programas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerTodosProgramas");

            try
            {
                await logger.InfoAsync("Iniciando búsqueda de todos los programas");

                var programas = await _programaRepository.ObtenerTodosAsync();

                await logger.InfoAsync($"Se encontraron {programas.Count} programas", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Programas obtenidos",
                    $"Se han obtenido {programas.Count} programas",
                    programas));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodosProgramas",
                    ex.Message);

                await logger.ErrorAsync("Error al obtener todos los programas", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene un programa por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ObtenerProgramaPorId-{id}");

            try
            {
                await logger.InfoAsync($"Buscando programa con ID: {id}");

                var programa = await _programaRepository.ObtenerPorIdAsync(id);

                if (programa == null)
                {
                    await logger.WarningAsync($"Programa no encontrado con ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Programa"));
                }

                await logger.InfoAsync($"Programa encontrado: {programa.Nombre}", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Programa obtenido",
                    $"Se ha obtenido el programa '{programa.Nombre}'",
                    programa));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerProgramaPorId: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al obtener programa con ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de programas
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerProgramasPaginado");

            try
            {
                await logger.InfoAsync($"Búsqueda paginada - Página: {pagina}, Elementos: {elementosPorPagina}, Búsqueda: '{busqueda}'");

                var programas = await _programaRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, busqueda);

                await logger.InfoAsync($"Resultado paginado - {programas.Lista?.Count ?? 0} programas de {programas.TotalRegistros} total", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Programas obtenidos",
                    $"Se han obtenido {programas.Lista?.Count ?? 0} programas de un total de {programas.TotalRegistros}",
                    programas));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerProgramasPaginado",
                    ex.Message);

                await logger.ErrorAsync("Error en búsqueda paginada de programas", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea un nuevo programa
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Crear([FromBody] ProgramaDto programaDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "CrearPrograma");

            try
            {
                await logger.InfoAsync($"Iniciando creación de programa - Nombre: {programaDto.Nombre}");

                var resultado = await _programaRepository.CrearAsync(programaDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearPrograma",
                        $"Se ha creado el programa '{programaDto.Nombre}'");

                    await logger.ActionAsync($"Programa creado exitosamente - Nombre: {programaDto.Nombre}");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((ProgramaDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en creación de programa: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "CrearPrograma",
                    ex.Message);

                await logger.ErrorAsync($"Error al crear programa - Nombre: {programaDto.Nombre}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza un programa existente
        /// </summary>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProgramaDto programaDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ActualizarPrograma-{id}");

            try
            {
                if (programaDto.Id != 0 && programaDto.Id != id)
                {
                    await logger.WarningAsync($"Discrepancia de ID - URL: {id}, DTO: {programaDto.Id}");
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID del programa no coincide con el ID de la URL"));
                }

                await logger.InfoAsync($"Iniciando actualización de programa - ID: {id}, Nombre: {programaDto.Nombre}");

                var existe = await _programaRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Programa no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Programa"));
                }

                var resultado = await _programaRepository.ActualizarAsync(id, programaDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarPrograma",
                        $"Se ha actualizado el programa '{programaDto.Nombre}'");

                    await logger.ActionAsync($"Programa actualizado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en actualización de programa: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ActualizarPrograma: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al actualizar programa - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina un programa
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"EliminarPrograma-{id}");

            try
            {
                await logger.InfoAsync($"Iniciando eliminación de programa - ID: {id}");

                var existe = await _programaRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Programa no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Programa"));
                }

                var resultado = await _programaRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarPrograma",
                        $"Se ha eliminado el programa con ID '{id}'");

                    await logger.ActionAsync($"Programa eliminado exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"No se pudo eliminar el programa: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"EliminarPrograma: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al eliminar programa - ID: {id}", ex);

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
