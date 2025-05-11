using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Domain.Contracts.MateriaRepository;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.MateriaInDto;

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
    public class MateriaController : ControllerBase
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly ILogRepository _logRepository;
        private readonly Domain.Contracts.ILoggerFactory _loggerFactory;

        public MateriaController(
            IMateriaRepository materiaRepository,
            ILogRepository logRepository,
            Domain.Contracts.ILoggerFactory loggerFactory)
        {
            _materiaRepository = materiaRepository;
            _logRepository = logRepository;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Obtiene todas las materias
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerTodasMaterias");

            try
            {
                await logger.InfoAsync("Iniciando búsqueda de todas las materias");

                var materias = await _materiaRepository.ObtenerTodosAsync();

                await logger.InfoAsync($"Se encontraron {materias.Count} materias", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Materias obtenidas",
                    $"Se han obtenido {materias.Count} materias",
                    materias));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodasMaterias",
                    ex.Message);

                await logger.ErrorAsync("Error al obtener todas las materias", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una materia por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ObtenerMateriaPorId-{id}");

            try
            {
                await logger.InfoAsync($"Buscando materia con ID: {id}");

                var materia = await _materiaRepository.ObtenerPorIdAsync(id);

                if (materia == null)
                {
                    await logger.WarningAsync($"Materia no encontrada con ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Materia"));
                }

                await logger.InfoAsync($"Materia encontrada: {materia.Codigo} - {materia.Nombre}", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Materia obtenida",
                    $"Se ha obtenido la materia '{materia.Codigo} - {materia.Nombre}'",
                    materia));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerMateriaPorId: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al obtener materia con ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de materias
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "ObtenerMateriasPaginado");

            try
            {
                await logger.InfoAsync($"Búsqueda paginada - Página: {pagina}, Elementos: {elementosPorPagina}, Búsqueda: '{busqueda}'");

                var materias = await _materiaRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, busqueda);

                await logger.InfoAsync($"Resultado paginado - {materias.Lista?.Count ?? 0} materias de {materias.TotalRegistros} total", logToDb: true);

                return Ok(RespuestaDto.Exitoso(
                    "Materias obtenidas",
                    $"Se han obtenido {materias.Lista?.Count ?? 0} materias de un total de {materias.TotalRegistros}",
                    materias));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerMateriasPaginado",
                    ex.Message);

                await logger.ErrorAsync("Error en búsqueda paginada de materias", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea una nueva materia
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Crear([FromBody] MateriaDto materiaDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), "CrearMateria");

            try
            {
                await logger.InfoAsync($"Iniciando creación de materia - Código: {materiaDto.Codigo}, Nombre: {materiaDto.Nombre}");

                var resultado = await _materiaRepository.CrearAsync(materiaDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearMateria",
                        $"Se ha creado la materia '{materiaDto.Codigo} - {materiaDto.Nombre}'");

                    await logger.ActionAsync($"Materia creada exitosamente - Código: {materiaDto.Codigo}");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((MateriaDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en creación de materia: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "CrearMateria",
                    ex.Message);

                await logger.ErrorAsync($"Error al crear materia - Código: {materiaDto.Codigo}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza una materia existente
        /// </summary>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Actualizar(int id, [FromBody] MateriaDto materiaDto)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"ActualizarMateria-{id}");

            try
            {
                if (materiaDto.Id != 0 && materiaDto.Id != id)
                {
                    await logger.WarningAsync($"Discrepancia de ID - URL: {id}, DTO: {materiaDto.Id}");
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID de la materia no coincide con el ID de la URL"));
                }

                await logger.InfoAsync($"Iniciando actualización de materia - ID: {id}, Código: {materiaDto.Codigo}");

                var existe = await _materiaRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Materia no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Materia"));
                }

                var resultado = await _materiaRepository.ActualizarAsync(id, materiaDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarMateria",
                        $"Se ha actualizado la materia '{materiaDto.Codigo} - {materiaDto.Nombre}'");

                    await logger.ActionAsync($"Materia actualizada exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"Fallo en actualización de materia: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ActualizarMateria: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al actualizar materia - ID: {id}", ex);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina una materia
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var logger = _loggerFactory.CreateLogger(GetUsuarioId().ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"EliminarMateria-{id}");

            try
            {
                await logger.InfoAsync($"Iniciando eliminación de materia - ID: {id}");

                var existe = await _materiaRepository.ExisteAsync(id);
                if (!existe)
                {
                    await logger.WarningAsync($"Materia no existe - ID: {id}");
                    return NotFound(RespuestaDto.NoEncontrado("Materia"));
                }

                var resultado = await _materiaRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarMateria",
                        $"Se ha eliminado la materia con ID '{id}'");

                    await logger.ActionAsync($"Materia eliminada exitosamente - ID: {id}");

                    return Ok(resultado);
                }
                else
                {
                    await logger.WarningAsync($"No se pudo eliminar la materia: {resultado.Detalle}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"EliminarMateria: {id}",
                    ex.Message);

                await logger.ErrorAsync($"Error al eliminar materia - ID: {id}", ex);

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
