namespace bolsafeucn_back.src.Domain.Models
{
    public class Image
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string PublicId { get; set; }

        public int PublicationId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
