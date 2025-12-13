namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Enum that defines the possible statuses of a job application.
    /// </summary>
    public enum ApplicationStatus
    {
        Pendiente,
        Aceptada,
        Rechazada
    }

    /// <summary>
    /// Represents an application submitted by a student to a job offer.
    /// </summary>
    public class JobApplication
    {
        /// <summary>
        /// Unique identifier for the application.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The student user who submitted the application.
        /// </summary>
        public required GeneralUser Student { get; set; }

        /// <summary>
        /// Identifier of the student who applied.
        /// </summary>
        public required int StudentId { get; set; }

        /// <summary>
        /// The job offer to which the student applied.
        /// </summary>
        public required Offer JobOffer { get; set; }

        /// <summary>
        /// Identifier of the job offer.
        /// </summary>
        public required int JobOfferId { get; set; }

        /// <summary>
        /// Current status of the application (Pendiente, Aceptada, Rechazada).
        /// </summary>
        public required ApplicationStatus Status { get; set; }

        /// <summary>
        /// Date and time when the application was submitted (UTC).
        /// </summary>
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    }
}
