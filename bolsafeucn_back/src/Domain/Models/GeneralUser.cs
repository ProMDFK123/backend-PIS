using Microsoft.AspNetCore.Identity;

namespace bolsafeucn_back.src.Domain.Models
{
    public enum UserType
    {
        Estudiante,
        Empresa,
        Particular,
        Administrador,
    }

    public class GeneralUser : IdentityUser<int>
    {
        public required UserType UserType { get; set; }
        public required string Rut { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public required bool Banned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public double Rating { get; set; } = 0.0;
        public ICollection<Publication> Publications { get; set; } = new List<Publication>();

        //Documentos
        public int? CVId { get; set; }
        public Curriculum? CV { get; set; }

        //Imagenes
        public int? ProfilePhotoId { get; set; }
        public UserImage? ProfilePhoto { get; set; } = null;
        public int? ProfileBannerId { get; set; }
        public UserImage? ProfileBanner { get; set; } = null;

        //Coneccion con los tipos de usuario
        public Student? Student { get; set; }
        public Company? Company { get; set; }
        public Individual? Individual { get; set; }
        public Admin? Admin { get; set; }
    }
}