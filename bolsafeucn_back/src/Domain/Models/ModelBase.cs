namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Clase base para los modelos de dominio que incluye propiedades comunes.
    /// </summary>
    public class ModelBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}