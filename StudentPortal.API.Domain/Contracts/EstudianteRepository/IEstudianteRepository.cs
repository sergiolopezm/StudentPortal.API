using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.EstudianteInDto;

namespace StudentPortal.API.Domain.Contracts.EstudianteRepository
{
    public interface IEstudianteRepository
    {
        Task<List<EstudianteDto>> ObtenerTodosAsync();
        Task<EstudianteDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(EstudianteDto estudianteDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, EstudianteDto estudianteDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorIdentificacionAsync(string identificacion);
        Task<PaginacionDto<EstudianteDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
        Task<List<CompaneroClaseDto>> ObtenerCompanerosPorEstudianteAsync(int estudianteId);
    }
}
