using System.ComponentModel.DataAnnotations;

namespace bolsafeucn_back.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs
{
    public class RequestResetPasswordCodeDTO
    {
        /// <summary>
        /// Correo electrónico del usuario que solicita el código de reseteo de contraseña.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public required string Email { get; set; }
    }
}
