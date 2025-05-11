using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class Materia
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int Creditos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public Guid? CreadoPorId { get; set; }

    public Guid? ModificadoPorId { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario? CreadoPor { get; set; }

    public virtual ICollection<InscripcionesEstudiante> InscripcionesEstudiantes { get; set; } = new List<InscripcionesEstudiante>();

    public virtual Usuario? ModificadoPor { get; set; }

    public virtual ICollection<ProfesorMateria> ProfesorMateria { get; set; } = new List<ProfesorMateria>();
}
