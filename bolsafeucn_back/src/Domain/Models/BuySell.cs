namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Represents a buy/sell publication for products or services.
    /// Inherits common publication properties from <see cref="Publication"/>.
    /// </summary>
    public class BuySell : Publication
    {
        /// <summary>
        /// Price of the product or service in Chilean pesos.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the product or service.
        /// </summary>
        public required string Category { get; set; }

        /// <summary>
        /// Location where the product or service is available.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Contact information for the listing.
        /// </summary>
        public string? ContactInfo { get; set; }
    }
}
