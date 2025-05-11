using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class Profesore
{
    public int Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Identificacion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Departamento { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ProfesorMateria> ProfesorMateria { get; set; } = new List<ProfesorMateria>();

    public virtual Usuario Usuario { get; set; } = null!;
}
