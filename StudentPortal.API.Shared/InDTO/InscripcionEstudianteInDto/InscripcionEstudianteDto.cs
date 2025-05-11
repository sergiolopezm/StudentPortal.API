using System.ComponentModel.DataAnnotations;

namespace StudentPortal.API.Shared.InDTO.InscripcionEstudianteInDto
{
    public class InscripcionEstudianteDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El estudiante es requerido")]
        public int EstudianteId { get; set; }

        [Required(ErrorMessage = "La materia es requerida")]
        public int MateriaId { get; set; }

        public DateTime FechaInscripcion { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Estado { get; set; } = "Inscrito";

        [Range(0.0, 5.0, ErrorMessage = "La calificación debe estar entre 0.0 y 5.0")]
        public decimal? Calificacion { get; set; }

        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedades de navegación para UI
        public string? EstudianteNombre { get; set; }
        public string? MateriaCodigoyNombre { get; set; }
        public int CredMateria { get; set; }
        public List<string>? Profesores { get; set; }
    }
}
