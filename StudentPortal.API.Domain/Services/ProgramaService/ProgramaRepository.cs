using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentPortal.API.Domain.Contracts.ProgramaRepository;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Shared.GeneralDTO;
using StudentPortal.API.Shared.InDTO.ProgramaInDto;
using StudentPortal.API.Util;

namespace StudentPortal.API.Domain.Services.ProgramaService
{
    public class ProgramaRepository : IProgramaRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<ProgramaRepository> _logger;

        public ProgramaRepository(DBContext context, ILogger<ProgramaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProgramaDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todos los programas");

            var programas = await _context.Programas
                .Where(p => p.Activo)
                .Include(p => p.Estudiantes)
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor)
                .AsNoTracking()
                .ToListAsync();

            var programasDto = Mapping.ConvertirLista<Programa, ProgramaDto>(programas);

            for (int i = 0; i < programas.Count; i++)
            {
                var entidad = programas[i];
                var dto = programasDto[i];

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;

                dto.EstudiantesCount = entidad.Estudiantes?.Count(e => e.Activo) ?? 0;
            }

            return programasDto.OrderBy(p => p.Nombre).ToList();
        }

        public async Task<ProgramaDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo programa con ID: {Id}", id);

            var programa = await _context.Programas
                .Where(p => p.Id == id)
                .Include(p => p.Estudiantes)
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (programa == null)
                return null;

            var dto = Mapping.Convertir<Programa, ProgramaDto>(programa);

            dto.CreadoPor = programa.CreadoPor != null
                ? $"{programa.CreadoPor.Nombre} {programa.CreadoPor.Apellido}"
                : null;

            dto.ModificadoPor = programa.ModificadoPor != null
                ? $"{programa.ModificadoPor.Nombre} {programa.ModificadoPor.Apellido}"
                : null;

            dto.EstudiantesCount = programa.Estudiantes?.Count(e => e.Activo) ?? 0;

            return dto;
        }

        public async Task<RespuestaDto> CrearAsync(ProgramaDto programaDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando programa: {Nombre}", programaDto.Nombre);

                // Validar que no exista un programa con el mismo nombre
                if (await _context.Programas.AnyAsync(p => p.Nombre == programaDto.Nombre))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un programa con el nombre '{programaDto.Nombre}'");
                }

                var programa = new Programa
                {
                    Nombre = programaDto.Nombre,
                    Descripcion = programaDto.Descripcion,
                    CreditosMinimos = programaDto.CreditosMinimos,
                    CreditosMaximos = programaDto.CreditosMaximos,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    CreadoPorId = usuarioId
                };

                await _context.Programas.AddAsync(programa);
                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Programa creado",
                    $"El programa '{programa.Nombre}' ha sido creado correctamente",
                    Mapping.Convertir<Programa, ProgramaDto>(programa));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear programa {Nombre}", programaDto.Nombre);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, ProgramaDto programaDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando programa con ID: {Id}", id);

                var programa = await _context.Programas.FindAsync(id);
                if (programa == null)
                {
                    return RespuestaDto.NoEncontrado("Programa");
                }

                // Validar que no exista otro programa con el mismo nombre
                if (programa.Nombre != programaDto.Nombre &&
                    await _context.Programas.AnyAsync(p => p.Nombre == programaDto.Nombre && p.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un programa con el nombre '{programaDto.Nombre}'");
                }

                programa.Nombre = programaDto.Nombre;
                programa.Descripcion = programaDto.Descripcion;
                programa.CreditosMinimos = programaDto.CreditosMinimos;
                programa.CreditosMaximos = programaDto.CreditosMaximos;
                programa.Activo = programaDto.Activo;
                programa.FechaModificacion = DateTime.Now;
                programa.ModificadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Programa actualizado",
                    $"El programa '{programa.Nombre}' ha sido actualizado correctamente",
                    Mapping.Convertir<Programa, ProgramaDto>(programa));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar programa {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando programa con ID: {Id}", id);

                var programa = await _context.Programas.Include(p => p.Estudiantes).FirstOrDefaultAsync(p => p.Id == id);
                if (programa == null)
                {
                    return RespuestaDto.NoEncontrado("Programa");
                }

                // Verificar si tiene estudiantes asociados
                if (programa.Estudiantes?.Any(e => e.Activo) ?? false)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar el programa '{programa.Nombre}' porque tiene estudiantes asociados");
                }

                programa.Activo = false;
                programa.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Programa eliminado",
                    $"El programa '{programa.Nombre}' ha sido eliminado correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar programa {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Programas.AnyAsync(p => p.Id == id);
        }

        public async Task<PaginacionDto<ProgramaDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo programas paginados. Página: {Pagina}, Elementos: {Elementos}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, busqueda);

            IQueryable<Programa> query = _context.Programas
                .Include(p => p.Estudiantes)
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(busqueda)));
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            List<Programa> programas = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            List<ProgramaDto> programasDto = Mapping.ConvertirLista<Programa, ProgramaDto>(programas);

            for (int i = 0; i < programas.Count; i++)
            {
                var entidad = programas[i];
                var dto = programasDto[i];

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;

                dto.EstudiantesCount = entidad.Estudiantes?.Count(e => e.Activo) ?? 0;
            }

            return new PaginacionDto<ProgramaDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = programasDto
            };
        }
    }
}
