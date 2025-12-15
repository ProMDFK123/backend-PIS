using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO resumido para listar publicaciones de compra/venta
    /// </summary>
    public class BuySellSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Location { get; set; }
        public DateTime PublicationDate { get; set; }
        public string? FirstImageUrl { get; set; }

        // Información del usuario
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public double UserRating { get; set; }
    }

    /// <summary>
    /// DTO detallado para ver una publicación de compra/venta específica
    /// </summary>
    public class BuySellDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Location { get; set; }
        public string? ContactInfo { get; set; }
        public DateTime PublicationDate { get; set; }
        public bool IsActive { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public required StatusValidation statusValidation { get; set; }

        // Información del usuario
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string AboutMe { get; set; }
        public double Rating { get; set; }
    }
}
