using bolsafeucn_back.src.Application.DTOs.PublicationDTO;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Mappers
{
    /// <summary>
    /// Mapper para convertir entidades de Publication a DTOs.
    /// Usado en el ReviewService para obtener información de publicaciones asociadas a reseñas.
    /// </summary>
    public static class PublicationMapper
    {
        /// <summary>
        /// Convierte una entidad Publication a PublicationsDTO.
        /// </summary>
        /// <param name="publication">La entidad Publication a convertir.</param>
        /// <returns>El DTO de la publicación.</returns>
        public static PublicationsDTO ToDTO(Publication publication)
        {
            return new PublicationsDTO
            {
                IdPublication = publication.Id,
                Title = publication.Title,
                types = publication.Type,
                Description = publication.Description,
                PublicationDate = publication.PublicationDate,
                Images = publication.Images,
                IsActive = publication.IsActive,
                statusValidation = publication.statusValidation
            };
        }
    }
}
