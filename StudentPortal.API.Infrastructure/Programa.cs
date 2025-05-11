using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class Programa
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int CreditosMinimos { get; set; }

    public int CreditosMaximos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public Guid? CreadoPorId { get; set; }

    public Guid? ModificadoPorId { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario? CreadoPor { get; set; }

    public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

    public virtual Usuario? ModificadoPor { get; set; }
}
