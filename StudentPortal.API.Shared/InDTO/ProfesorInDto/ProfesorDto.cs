using System.ComponentModel.DataAnnotations;

namespace StudentPortal.API.Shared.InDTO.ProfesorInDto
{
    public class ProfesorDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        [StringLength(50, ErrorMessage = "La identificación no puede exceder los 50 caracteres")]
        public string Identificacion { get; set; } = null!;

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El departamento no puede exceder los 100 caracteres")]
        public string? Departamento { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Propiedades de navegación para UI
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public List<string>? Materias { get; set; }
        public int MateriasCount { get; set; }
    }
}
