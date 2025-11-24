using bolsafeucn_back.src.Application.DTOs.OfferDTOs;
using bolsafeucn_back.src.Application.DTOs.PublicationDTO;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace bolsafeucn_back.src.Application.Services.Implements;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly ILogger<OfferService> _logger;
    private readonly AppDbContext _context;

    public OfferService(
        IOfferRepository offerRepository,
        ILogger<OfferService> logger,
        AppDbContext context
    )
    {
        _offerRepository = offerRepository;
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<OfferSummaryDto>> GetActiveOffersAsync()
    {
        _logger.LogInformation("Obteniendo todas las ofertas activas");

        // Debe traer User + Company/Individual (el repo debería hacer Include).
        var offers = await _offerRepository.GetAllActiveAsync();
        var list = offers.ToList();

        _logger.LogInformation("Se encontraron {Count} ofertas activas", list.Count);

        var result = list.Select(o =>
            {
                // Nombre de oferente
                var ownerName =
                    o.User?.UserType == UserType.Empresa
                        ? (o.User.Company?.CompanyName ?? "Empresa desconocida")
                    : o.User?.UserType == UserType.Particular
                        ? $"{(o.User.Individual?.Name ?? "").Trim()} {(o.User.Individual?.LastName ?? "").Trim()}".Trim()
                    : (o.User?.UserName ?? "UCN");

                return new OfferSummaryDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    CompanyName = ownerName, // si lo sigues usando en otros lados
                    OwnerName = ownerName, // lo que consume el front para “oferente”

                    Location = "Campus Antofagasta",

                    // 💰 y fechas para la tarjeta
                    Remuneration = o.Remuneration,
                    DeadlineDate = o.DeadlineDate,
                    PublicationDate = o.PublicationDate,
                    OfferType = o.OfferType, // Trabajo / Voluntariado (enum)
                };
            })
            .ToList();

        return result;
    }

    public async Task<OfferDetailDto?> GetOfferDetailsAsync(int offerId)
    {
        _logger.LogInformation("Obteniendo detalles de la oferta ID: {OfferId}", offerId);

        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            _logger.LogWarning("Oferta con ID {OfferId} no encontrada", offerId);
            throw new KeyNotFoundException($"Offer with id {offerId} not found");
        }

        // Nombre de oferente para detalles
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.Company?.CompanyName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.Individual?.Name ?? "").Trim()} {(offer.User.Individual?.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "UCN");

        var result = new OfferDetailDto
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            CompanyName = ownerName,
            // si también quieres forzar aquí Antofagasta:
            Location = "Campus Antofagasta",

            PostDate = offer.PublicationDate,
            EndDate = offer.EndDate,
            Remuneration = (int)offer.Remuneration, // tu DTO usa int
            OfferType = offer.OfferType.ToString(),
        };

        _logger.LogInformation("Detalles de oferta ID: {OfferId} obtenidos exitosamente", offerId);
        return result;
    }

    public async Task PublishOfferAsync(int id)
    {
        var offer = await _context.Offers.FindAsync(id);
        if (offer == null)
            throw new KeyNotFoundException("Offer not found.");

        offer.IsActive = true; // o Published / Active, según tu modelo
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync();
    }

    public async Task RejectOfferAsync(int id)
    {
        var offer = await _context.Offers.FindAsync(id);
        if (offer == null)
            throw new KeyNotFoundException("Offer not found.");

        offer.IsActive = false;
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PendingOffersForAdminDto>> GetPendingOffersAsync()
    {
        var offer = await _offerRepository.GetAllPendingOffersAsync();
        return offer
            .Select(o => new PendingOffersForAdminDto
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                Location = o.Location,
                PostDate = o.PublicationDate,
                Remuneration = o.Remuneration,
            })
            .ToList();
    }

    public async Task<IEnumerable<OfferBasicAdminDto>> GetPublishedOffersAsync()
    {
        var offer = await _offerRepository.PublishedOffersAsync();
        var list = offer.ToList();
        var result = list.Select(o =>
            {
                var ownerName =
                    o.User?.UserType == UserType.Empresa
                        ? (o.User.Company?.CompanyName ?? "Empresa desconocida")
                    : o.User?.UserType == UserType.Particular
                        ? $"{(o.User.Individual?.Name ?? "").Trim()} {(o.User.Individual?.LastName ?? "").Trim()}".Trim()
                    : (o.User?.UserName ?? "UCN");
                return new OfferBasicAdminDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    CompanyName = ownerName,
                    PublicationDate = o.PublicationDate,
                    OfferType = o.OfferType,
                    Activa = o.IsActive,
                    Remuneration = o.Remuneration,
                };
            })
            .ToList();
        return result;
    }

    public async Task<OfferDetailsAdminDto> GetOfferDetailsForAdminManagement(int offerId)
    {
        _logger.LogInformation("Obteniendo detalles de la oferta ID: {OfferId}", offerId);
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            _logger.LogWarning("Oferta con ID {OfferId} no encontrada", offerId);
            throw new KeyNotFoundException($"Offer with id {offerId} not found");
        }
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.Company?.CompanyName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.Individual?.Name ?? "").Trim()} {(offer.User.Individual?.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "UCN");
        var imageForDTO = offer.Images.Select(i => i.Url).ToList();
        var result = new OfferDetailsAdminDto
        {
            Title = offer.Title,
            Description = offer.Description,
            Images = imageForDTO,
            CompanyName = ownerName,
            PublicationDate = offer.PublicationDate,
            EndDate = offer.EndDate,
            DeadlineDate = offer.DeadlineDate,
            Type = offer.Type,
            Active = offer.IsActive,
            statusValidation = offer.statusValidation,
            Remuneration = offer.Remuneration,
        };
        _logger.LogInformation("Detalles de oferta ID: {OfferId} obtenidos exitosamente", offerId);
        return result;
    }

    public async Task GetOfferForAdminToClose(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }
        if (!offer.IsActive)
        {
            throw new InvalidOperationException($"La oferta con id {offerId} ya ha sido cerrada.");
        }
        offer.IsActive = false;
        await _offerRepository.UpdateOfferAsync(offer);
    }

    public async Task<OfferDetailValidationDto> GetOfferDetailForOfferValidationAsync(int id)
    {
        _logger.LogInformation("Obteniendo detalles de la oferta ID: {OfferId}", id);
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            _logger.LogWarning("Oferta con ID {OfferId} no encontrada", id);
            throw new KeyNotFoundException($"Offer with id {id} not found");
        }
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.Company?.CompanyName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.Individual?.Name ?? "").Trim()} {(offer.User.Individual?.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "UCN");
        var imageForDTO = offer.Images.Select(i => i.Url).ToList();
        return new OfferDetailValidationDto
        {
            Title = offer.Title,
            Images = imageForDTO,
            Description = offer.Description,
            CompanyName = ownerName,
            CorreoContacto = offer.ContactInfo,
            TelefonoContacto = offer.User?.PhoneNumber,
            PublicationDate = offer.PublicationDate,
            DeadlineDate = offer.DeadlineDate,
            EndDate = offer.EndDate,
            Remuneration = offer.Remuneration,
        };
    }

    public async Task GetOfferForAdminToPublish(int id)
    {
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }
        if (offer.statusValidation != StatusValidation.InProcess)
        {
            throw new InvalidOperationException(
                $"La oferta con ID {id} ya fue {offer.statusValidation}. No se puede publicar."
            );
        }
        offer.IsActive = true;
        offer.statusValidation = StatusValidation.Published;
        await _offerRepository.UpdateOfferAsync(offer);
    }

    public async Task GetOfferForAdminToReject(int id)
    {
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }
        if (offer.statusValidation != StatusValidation.InProcess)
        {
            throw new InvalidOperationException(
                $"La oferta con ID {id} ya fue {offer.statusValidation}. No se puede rechazar."
            );
        }
        offer.IsActive = false;
        offer.statusValidation = StatusValidation.Rejected;
        await _offerRepository.UpdateOfferAsync(offer);
    }

    public async Task<OfferDetailDto> GetOfferDetailForOfferer(int id, string userId)
    {
        var offer = await _offerRepository.GetByIdAsync(id);

        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }

        if (!int.TryParse(userId, out int parsedUserId))
        {
            throw new UnauthorizedAccessException("El ID de usuario es inválido.");
        }

        if (offer.UserId != parsedUserId)
        {
            // Lanza 404 para no revelar que la oferta existe pero no es suya
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");

            // throw new UnauthorizedAccessException("No tienes permiso para ver esta oferta.");
        }
        var offerDetailDto = offer.Adapt<OfferDetailDto>();

        return offerDetailDto;
    }

    public async Task ClosePublishedOfferAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }
        if (!offer.IsActive)
        {
            throw new InvalidOperationException($"La oferta con id {offerId} ya ha sido cerrada.");
        }
        offer.IsActive = false;
        await _offerRepository.UpdateOfferAsync(offer);
    }
}
