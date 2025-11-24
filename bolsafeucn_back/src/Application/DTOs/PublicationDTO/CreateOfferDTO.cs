using System.ComponentModel.DataAnnotations;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO para la creación de ofertas laborales o de voluntariado
    /// </summary>
    public class CreateOfferDTO : IValidatableObject
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "El título debe tener entre 5 y 200 caracteres"
        )]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres"
        )]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de finalización del trabajo es obligatoria")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "La fecha Inicial de trabajo es obligatoria")]
        public DateTime DeadlineDate { get; set; }

        [Required(ErrorMessage = "La remuneración es obligatoria")]
        [Range(0, 100000000, ErrorMessage = "La remuneración debe estar entre $0 y $100.000.000")]
        public decimal Remuneration { get; set; }

        [Required(ErrorMessage = "El tipo de oferta es obligatorio")]
        [Range(0, 1, ErrorMessage = "El Tipo debe ser 1 (Voluntario) o 0 (Oferta)")]
        public OfferTypes OfferType { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Los requisitos no pueden exceder 1000 caracteres")]
        public string? Requirements { get; set; }

        [StringLength(
            200,
            ErrorMessage = "La información de contacto no puede exceder 200 caracteres"
        )]
        public string? ContactInfo { get; set; }

        [MaxLength(10, ErrorMessage = "Máximo 10 imágenes permitidas")]
        public List<string> ImagesURL { get; set; } = new();

        /// <summary>
        /// Indica si el CV es obligatorio para postular a esta oferta
        /// Por defecto es true (obligatorio)
        /// </summary>
        public bool IsCvRequired { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var now = DateTime.UtcNow;

            if (DeadlineDate <= now)
            {
                yield return new ValidationResult(
                    "La fecha límite debe ser posterior a la fecha actual",
                    new[] { nameof(DeadlineDate) }
                );
            }

            if (EndDate <= now)
            {
                yield return new ValidationResult(
                    "La fecha de finalización debe ser posterior a la fecha actual",
                    new[] { nameof(EndDate) }
                );
            }

            if (EndDate <= DeadlineDate)
            {
                yield return new ValidationResult(
                    "La fecha de finalización debe ser posterior a la fecha límite de postulación",
                    new[] { nameof(EndDate) }
                );
            }

            if (OfferType == OfferTypes.Voluntariado && Remuneration > 0)
            {
                yield return new ValidationResult(
                    "Un voluntariado no puede tener remuneración mayor a 0",
                    new[] { nameof(Remuneration) }
                );
            }
        }
    }
}
