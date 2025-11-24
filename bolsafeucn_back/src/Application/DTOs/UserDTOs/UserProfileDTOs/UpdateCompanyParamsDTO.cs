using System.ComponentModel.DataAnnotations;
using bolsafeucn_back.src.Application.Validators;
using bolsafeucn_back.src.Domain.Models;
using Mapster;

namespace bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class UpdateCompanyParamsDTO : IUpdateParamsDTO
    {
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Primer nombre del usuario.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Segundo nombre del usuario.
        /// </summary>
        public string? LegalName { get; set; }

        /// <summary>
        /// RUT del usuario.
        /// </summary>
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El RUT no es válido.")]      
        public string? Rut { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string? PhoneNumber { get; set; } 

        /// <summary>
        /// Información sobre el usuario.
        /// </summary>
        [MaxLength(200, ErrorMessage = "La información sobre el usuario debe tener como máximo 200 caracteres")]
        public string? AboutMe { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public string? Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña del usuario.
        /// </summary>
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; } 

        /// <summary>
        /// Imagen de perfil del usuario.
        /// </summary>
        public IFormFile? ProfilePhoto { get; set; }

        /// <summary>
        /// Banner de perfil del usuario.
        /// </summary>
        public IFormFile? ProfileBanner { get; set; }

        public void ApplyTo(GeneralUser user)
        {
            this.Adapt(user);
        }
        public void ApplyTo(UserImagesDTO imagesDTO)
        {
            imagesDTO.ProfilePhoto = this.ProfilePhoto;
            imagesDTO.ProfilePhoto = this.ProfileBanner;
        }
    }
}