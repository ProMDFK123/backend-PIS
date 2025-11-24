using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bolsafeucn_back.src.Application.DTOs.OfferDTOs
{
    /// <summary>
    /// <summary>
    /// DTO that contains all information required by administrators to validate an offer.
    /// </summary>
    /// TODO: add company description
    public class OfferDetailValidationDto
    {
        /// <summary>
        /// Offer title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Collection of image URLs attached to the offer.
        /// </summary>
        public required ICollection<string> Images { get; set; }

        /// <summary>
        /// Full description of the offer.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Name of the company or individual who posted the offer.
        /// </summary>
        public required string CompanyName { get; set; }

        /// <summary>
        /// Contact email for the offer.
        /// </summary>
        public required string CorreoContacto { get; set; }

        /// <summary>
        /// Contact phone number for the offer.
        /// </summary>
        public required string TelefonoContacto { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DeadlineDate { get; set; }
        public DateTime PublicationDate { get; set; }
        public int Remuneration { get; set; }

    }
}