using Mapster;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Application.DTOs.PublicationDTO;

/// <summary>
/// Configuración de mapeos entre entidades Offer y sus DTOs usando Mapster
/// </summary>
namespace bolsafeucn_back.src.Application.Mappers
{
    public class BuySellMapper
    {
        /// <summary>
        /// Configura todos los mapeos relacionados con BuySell
        /// </summary>
        public void ConfigureAllMappings()
        {
            TypeAdapterConfig<BuySell, BuySellSummaryDto>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.PublicationDate, src => src.PublicationDate)
                .Map(dest => dest.UserName, src => src.User.UserName);


            TypeAdapterConfig<BuySell, BuySellDetailDto>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.ContactInfo, src => src.ContactInfo)
                .Map(dest => dest.PublicationDate, src => src.PublicationDate)
                .Map(dest => dest.UserName, src => src.User.UserName)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}