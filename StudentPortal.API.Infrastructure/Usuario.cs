using System;
using System.Collections.Generic;

namespace StudentPortal.API.Infrastructure;

public partial class Usuario
{
    public Guid Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public virtual Estudiante? Estudiante { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Materia> MateriaCreadoPors { get; set; } = new List<Materia>();

    public virtual ICollection<Materia> MateriaModificadoPors { get; set; } = new List<Materia>();

    public virtual Profesore? Profesore { get; set; }

    public virtual ICollection<Programa> ProgramaCreadoPors { get; set; } = new List<Programa>();

    public virtual ICollection<Programa> ProgramaModificadoPors { get; set; } = new List<Programa>();

    public virtual Role Rol { get; set; } = null!;

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public virtual ICollection<TokensExpirado> TokensExpirados { get; set; } = new List<TokensExpirado>();
}
