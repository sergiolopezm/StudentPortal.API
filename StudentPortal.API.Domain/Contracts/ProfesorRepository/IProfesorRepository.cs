using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProfesorInDto;

namespace StudentPortal.API.Domain.Contracts.ProfesorRepository
{
    public interface IProfesorRepository
    {
        Task<List<ProfesorDto>> ObtenerTodosAsync();
        Task<ProfesorDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(ProfesorDto profesorDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, ProfesorDto profesorDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorIdentificacionAsync(string identificacion);
        Task<PaginacionDto<ProfesorDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
    }
}
