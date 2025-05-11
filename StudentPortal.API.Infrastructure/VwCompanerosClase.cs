using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class VwCompanerosClase
{
    public int EstudianteId { get; set; }

    public int MateriaId { get; set; }

    public string MateriaNombre { get; set; } = null!;

    public int CompaneroId { get; set; }

    public string CompaneroNombre { get; set; } = null!;
}
