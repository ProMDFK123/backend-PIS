using System.ComponentModel.DataAnnotations;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO containing the user's justification for appealing a rejection.
    /// </summary>
    public class UserAppealDto
    {
        /// <summary>
        /// The user's argument or correction details for the appeal.
        /// </summary>
        [Required(ErrorMessage = "Debe justificar su apelación.")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "La justificación debe tener entre 20 y 1000 caracteres.")]
        public required string Justification { get; set; }
    }
}