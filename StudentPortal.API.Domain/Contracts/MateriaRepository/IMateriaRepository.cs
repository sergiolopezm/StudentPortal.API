using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.MateriaInDto;

namespace StudentPortal.API.Domain.Contracts.MateriaRepository
{
    public interface IMateriaRepository
    {
        Task<List<MateriaDto>> ObtenerTodosAsync();
        Task<MateriaDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(MateriaDto materiaDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, MateriaDto materiaDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorCodigoAsync(string codigo);
        Task<PaginacionDto<MateriaDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
    }
}
