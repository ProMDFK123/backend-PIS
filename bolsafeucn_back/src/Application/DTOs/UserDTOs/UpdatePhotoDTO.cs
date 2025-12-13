using System.ComponentModel.DataAnnotations;

namespace bolsafeucn_back.src.Application.DTOs.UserDTOs
{
    public class UpdatePhotoDTO
    {
        [Required(ErrorMessage = "La foto es obligatoria")]
        public IFormFile Photo { get; set; } = null!;
    }
}