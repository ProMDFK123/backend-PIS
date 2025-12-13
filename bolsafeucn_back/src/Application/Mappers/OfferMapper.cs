using bolsafeucn_back.src.Application.DTOs.OfferDTOs;
using bolsafeucn_back.src.Domain.Models;
using Mapster;

namespace bolsafeucn_back.src.Application.Mappers;

/// <summary>
/// Configures mappings between the domain <see cref="Domain.Models.Offer"/> entity and its DTO types using Mapster.
/// This class centralizes mapping rules used across the application to keep mapping logic consistent.
/// </summary>
public class OfferMapper
{
    /// <summary>
    /// Registers all Mapster mapping configurations for <see cref="Domain.Models.Offer"/>.
    /// Call this during application startup to ensure DTO mappings are available.
    /// </summary>
    public void ConfigureAllMappings()
    {
        // Map Offer to OfferSummaryDto (used for lists)
        TypeAdapterConfig<Offer, OfferSummaryDto>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(
                dest => dest.CompanyName,
                src =>
                    src.User.UserType == UserType.Empresa ? src.User.Company!.CompanyName
                    : src.User.UserType == UserType.Particular
                        ? $"{src.User.Individual!.Name} {src.User.Individual!.LastName}"
                    : "Unknown"
            );

        // Map Offer to OfferDetailDto (full details)
        TypeAdapterConfig<Offer, OfferDetailDto>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PostDate, src => src.PublicationDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Remuneration, src => src.Remuneration)
            .Map(dest => dest.OfferType, src => src.OfferType.ToString())
            .Map(
                dest => dest.CompanyName,
                src =>
                    src.User.UserType == UserType.Empresa ? src.User.Company!.CompanyName
                    : src.User.UserType == UserType.Particular
                        ? $"{src.User.Individual!.Name} {src.User.Individual!.LastName}"
                    : "Unknown"
            );
    }
}
