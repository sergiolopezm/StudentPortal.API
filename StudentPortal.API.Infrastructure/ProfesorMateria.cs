using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class ProfesorMateria
{
    public int Id { get; set; }

    public int ProfesorId { get; set; }

    public int MateriaId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual Materia Materia { get; set; } = null!;

    public virtual Profesore Profesor { get; set; } = null!;
}
