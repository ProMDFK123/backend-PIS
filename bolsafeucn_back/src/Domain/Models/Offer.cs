namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Enum that defines available offer categories in the system.
    /// </summary>
    public enum OfferTypes
    {
        Trabajo,
        Voluntariado,
    }

    /// <summary>
    /// Represents a job or volunteer offer published in the system.
    /// Inherits common publication properties from <see cref="Publication"/>.
    /// </summary>
    public class Offer : Publication
    {
        /// <summary>
        /// End date of the offer (when the position or opportunity ends).
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Application deadline date for the offer.
        /// </summary>
        public DateTime DeadlineDate { get; set; }

        /// <summary>
        /// Offered remuneration in Chilean pesos. Use 0 for volunteer positions.
        /// </summary>
        public required int Remuneration { get; set; }

        /// <summary>
        /// Offer category (e.g., Trabajo, Voluntariado).
        /// </summary>
        public required OfferTypes OfferType { get; set; }

        /// <summary>
        /// Job location (city, region, remote, etc.). Optional.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Specific requirements for the offer (skills, education, experience).
        /// </summary>
        public string? Requirements { get; set; }

        /// <summary>
        /// Contact information (email or phone) for the offer.
        /// </summary>
        public string? ContactInfo { get; set; }

        /// <summary>
        /// Indicates whether uploading a CV is required to apply.
        /// true = CV required; false = CV optional.
        /// Default is true.
        /// </summary>
        public bool IsCvRequired { get; set; } = true;
    }
}
