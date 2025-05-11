using System.ComponentModel.DataAnnotations;

namespace StudentPortal.API.Shared.InDTO.MateriaInDto
{
    public class MateriaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de la materia es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder los 20 caracteres")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de la materia es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayor a 0")]
        public int Creditos { get; set; } = 3;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorId { get; set; }
        public Guid? ModificadoPorId { get; set; }

        // Propiedades de navegación para UI
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
        public int InscripcionesCount { get; set; }
        public List<string>? Profesores { get; set; }
    }
}
