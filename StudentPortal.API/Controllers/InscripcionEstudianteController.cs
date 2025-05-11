using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Domain.Contracts.InscripcionEstudianteRepository;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.InscripcionEstudianteInDto;

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
    public class InscripcionEstudianteController : ControllerBase
    {
        private readonly IInscripcionEstudianteRepository _inscripcionRepository;
        private readonly ILogRepository _logRepository;

        public InscripcionEstudianteController(
            IInscripcionEstudianteRepository inscripcionRepository,
            ILogRepository logRepository)
        {
            _inscripcionRepository = inscripcionRepository;
            _logRepository = logRepository;
        }

        /// <summary>
        /// Obtiene las inscripciones de un estudiante
        /// </summary>
        [HttpGet("estudiante/{estudianteId}")]
        public async Task<IActionResult> ObtenerPorEstudiante(int estudianteId)
        {
            try
            {
                var inscripciones = await _inscripcionRepository.ObtenerPorEstudianteAsync(estudianteId);
                return Ok(RespuestaDto.Exitoso(
                    "Inscripciones obtenidas",
                    $"Se han obtenido {inscripciones.Count} inscripciones del estudiante",
                    inscripciones));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerInscripcionesPorEstudiante: {estudianteId}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene las inscripciones de una materia
        /// </summary>
        [HttpGet("materia/{materiaId}")]
        public async Task<IActionResult> ObtenerPorMateria(int materiaId)
        {
            try
            {
                var inscripciones = await _inscripcionRepository.ObtenerPorMateriaAsync(materiaId);
                return Ok(RespuestaDto.Exitoso(
                    "Inscripciones obtenidas",
                    $"Se han obtenido {inscripciones.Count} inscripciones de la materia",
                    inscripciones));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerInscripcionesPorMateria: {materiaId}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una inscripción por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var inscripcion = await _inscripcionRepository.ObtenerPorIdAsync(id);

                if (inscripcion == null)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Inscripción"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Inscripción obtenida",
                    "Se ha obtenido la inscripción correctamente",
                    inscripcion));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerInscripcionPorId: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de inscripciones
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] int? estudianteId = null,
            [FromQuery] int? materiaId = null)
        {
            try
            {
                var inscripciones = await _inscripcionRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, estudianteId, materiaId);

                return Ok(RespuestaDto.Exitoso(
                    "Inscripciones obtenidas",
                    $"Se han obtenido {inscripciones.Lista?.Count ?? 0} inscripciones de un total de {inscripciones.TotalRegistros}",
                    inscripciones));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerInscripcionesPaginado",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Inscribe un estudiante en una materia
        /// </summary>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<IActionResult> Inscribir([FromBody] InscripcionEstudianteDto inscripcionDto)
        {
            try
            {
                var resultado = await _inscripcionRepository.InscribirAsync(inscripcionDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "InscribirEstudiante",
                        $"Se ha inscrito el estudiante ID {inscripcionDto.EstudianteId} en la materia ID {inscripcionDto.MateriaId}");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((InscripcionEstudianteDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "InscribirEstudiante",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Califica una inscripción
        /// </summary>
        [HttpPut("{id}/calificar")]
        public async Task<IActionResult> Calificar(int id, [FromQuery] decimal calificacion)
        {
            try
            {
                if (calificacion < 0 || calificacion > 5)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Calificación inválida",
                        "La calificación debe estar entre 0.0 y 5.0"));
                }

                var resultado = await _inscripcionRepository.CalificarAsync(id, calificacion, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CalificarInscripcion",
                        $"Se ha calificado la inscripción ID {id} con {calificacion}");

                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"CalificarInscripcion: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Cancela una inscripción
        /// </summary>
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var resultado = await _inscripcionRepository.CancelarInscripcionAsync(id, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CancelarInscripcion",
                        $"Se ha cancelado la inscripción ID {id}");

                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"CancelarInscripcion: {id}",
                    ex.Message);

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
