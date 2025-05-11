using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class Estudiante
{
    public int Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Identificacion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Carrera { get; set; }

    public int ProgramaId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<InscripcionesEstudiante> InscripcionesEstudiantes { get; set; } = new List<InscripcionesEstudiante>();

    public virtual Programa Programa { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
