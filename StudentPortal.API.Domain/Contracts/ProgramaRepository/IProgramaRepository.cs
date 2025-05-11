using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProgramaInDto;

namespace StudentPortal.API.Domain.Contracts.ProgramaRepository
{
    public interface IProgramaRepository
    {
        Task<List<ProgramaDto>> ObtenerTodosAsync();
        Task<ProgramaDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(ProgramaDto programaDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, ProgramaDto programaDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<PaginacionDto<ProgramaDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
    }
}
