namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Enum que representa los tipos de discapacidad.
    /// </summary>
    public enum Disability
    {
        Ninguna,
        Visual,
        Auditiva,
        Motriz,
        Cognitiva,
        Otra,
    }

    /// <summary>
    /// Clase que representa un estudiante.
    /// </summary>
    public class Student
    {
        public int Id { get; set; }
        public required GeneralUser GeneralUser { get; set; }
        public required int GeneralUserId { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required Disability Disability { get; set; }
        public string CurriculumVitae { get; set; } = string.Empty;
        public string MotivationLetter { get; set; } = string.Empty;
    }
}
