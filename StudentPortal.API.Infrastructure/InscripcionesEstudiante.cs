using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class InscripcionesEstudiante
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }

    public int MateriaId { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public string Estado { get; set; } = null!;

    public decimal? Calificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Estudiante Estudiante { get; set; } = null!;

    public virtual Materia Materia { get; set; } = null!;
}
