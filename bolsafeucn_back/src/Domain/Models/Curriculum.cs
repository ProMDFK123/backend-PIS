namespace bolsafeucn_back.src.Domain.Models
{
    public class Curriculum
    {
        public int Id { get; set; }
        // Metadata del archivo
        public required string OriginalFileName { get; set; }
        public long FileSizeBytes { get; set; }
        // Storage
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}