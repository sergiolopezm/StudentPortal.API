using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.InscripcionEstudianteInDto;

namespace StudentPortal.API.Domain.Contracts.InscripcionEstudianteRepository
{
    public interface IInscripcionEstudianteRepository
    {
        Task<List<InscripcionEstudianteDto>> ObtenerPorEstudianteAsync(int estudianteId);
        Task<List<InscripcionEstudianteDto>> ObtenerPorMateriaAsync(int materiaId);
        Task<InscripcionEstudianteDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> InscribirAsync(InscripcionEstudianteDto inscripcionDto, Guid usuarioId);
        Task<RespuestaDto> CalificarAsync(int id, decimal calificacion, Guid usuarioId);
        Task<RespuestaDto> CancelarInscripcionAsync(int id, Guid usuarioId);
        Task<bool> ExisteInscripcionAsync(int estudianteId, int materiaId);
        Task<int> ContarMateriasInscritasAsync(int estudianteId);
        Task<bool> ValidarProfesorDiferenteAsync(int estudianteId, int materiaId);
        Task<PaginacionDto<InscripcionEstudianteDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? estudianteId = null, int? materiaId = null);
    }
}
