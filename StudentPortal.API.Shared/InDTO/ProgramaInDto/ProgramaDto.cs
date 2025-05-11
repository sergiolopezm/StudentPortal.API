using System.ComponentModel.DataAnnotations;

namespace StudentPortal.API.Shared.InDTO.ProgramaInDto
{
    public class ProgramaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del programa es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los créditos mínimos no pueden ser negativos")]
        public int CreditosMinimos { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos máximos deben ser mayor a 0")]
        public int CreditosMaximos { get; set; } = 9;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorId { get; set; }
        public Guid? ModificadoPorId { get; set; }

        // Propiedades de navegación para UI
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
        public int EstudiantesCount { get; set; }
    }
}
