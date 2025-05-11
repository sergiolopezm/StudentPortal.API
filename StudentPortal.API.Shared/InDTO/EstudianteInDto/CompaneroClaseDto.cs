namespace StudentPortal.API.Shared.InDTO.EstudianteInDto
{
    public class CompaneroClaseDto
    {
        public int EstudianteId { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = null!;
        public int CompaneroId { get; set; }
        public string CompaneroNombre { get; set; } = null!;
    }
}
