namespace bolsafeucn_back.src.Domain.Models
{
    public enum UserImageType
    {
        Perfil,
        Banner,
    }
    public class UserImage
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        public required UserImageType ImageType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}