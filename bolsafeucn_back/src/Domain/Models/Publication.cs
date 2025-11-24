namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Enum that defines types of publications in the system.
    /// </summary>
    public enum Types
    {
        Offer, // Job or volunteer offer
        BuySell, // Buy/Sell listing
    }

    /// <summary>
    /// Validation state used by administrative workflows.
    /// </summary>
    public enum StatusValidation
    {
        Published, // Validated and published by an administrator
        InProcess, // Under administrative review
        Rejected, // Rejected by an administrator
        Closed, // Closed by the user or administrator
    }

    /// <summary>
    /// Abstract base class for all publication entities in the system.
    /// Derived types include <see cref="Offer"/> and <see cref="BuySell"/>.
    /// </summary>
    public abstract class Publication
    {
        /// <summary>
        /// Unique identifier for the publication.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user who created the publication.
        /// </summary>
        public required GeneralUser User { get; set; }

        /// <summary>
        /// Identifier of the user who created the publication.
        /// </summary>
        public required int UserId { get; set; }

        /// <summary>
        /// Title of the publication.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Full description of the publication.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Publication date and time in UTC.
        /// </summary>
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of images attached to the publication.
        /// </summary>
        public ICollection<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Publication type (Offer, BuySell).
        /// </summary>
        public required Types Type { get; set; }

        /// <summary>
        /// Whether the publication is active and visible to users.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Administrative validation status for the publication.
        /// </summary>
        public StatusValidation statusValidation { get; set; }
    }
}
