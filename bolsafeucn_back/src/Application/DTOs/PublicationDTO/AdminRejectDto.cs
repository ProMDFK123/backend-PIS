using System.ComponentModel.DataAnnotations;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO containing the reason for rejecting a publication.
    /// </summary>
    public class AdminRejectDto
    {
        /// <summary>
        /// The explanation of why the publication was rejected.
        /// </summary>
        [Required(ErrorMessage = "Debe proporcionar una razón para el rechazo.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "La razón debe tener entre 10 y 500 caracteres.")]
        public required string Reason { get; set; }
    }
}