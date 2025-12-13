using System.ComponentModel.DataAnnotations;
using bolsafeucn_back.src.Application.Validators;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Application.DTOs.UserDTOs;
using Mapster;

namespace bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public class UpdateStudentParamsDTO : IUpdateParamsDTO
    {
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        /// 
        public string? UserName { get; set; }

        /// <summary>
        /// Primer nombre del usuario.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Segundo nombre del usuario.
        /// </summary>
        public string? LastName { get; set; }

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
        [MaxLength(500, ErrorMessage = "La información sobre el usuario debe tener como máximo 500 caracteres")]
        public string? AboutMe { get; set; }

        /// <summary>
        /// Aplica los cambios del DTO al usuario dado.
        /// </summary>
        /// <param name="user">Usuario al que se le aplicarán los cambios.</param>
        public void ApplyTo(GeneralUser user)
        {
            this.Adapt(user);
        }
    }
}