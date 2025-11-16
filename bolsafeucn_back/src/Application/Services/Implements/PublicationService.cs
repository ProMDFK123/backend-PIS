using bolsafeucn_back.src.Application.DTOs.BaseResponse;
using bolsafeucn_back.src.Application.DTOs.PublicationDTO;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para la gestión de publicaciones (Ofertas y Compra/Venta)
    /// </summary>
    public class PublicationService : IPublicationService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IBuySellRepository _buySellRepository;
        private readonly ILogger<PublicationService> _logger;

        private readonly IPublicationRepository _publicationRepository;
        private readonly IMapper _mapper;

        public PublicationService(
            IOfferRepository offerRepository,
            IBuySellRepository buySellRepository,
            ILogger<PublicationService> logger,
            IPublicationRepository publicationRepository,
            IMapper mapper
        )
        {
            _offerRepository = offerRepository;
            _buySellRepository = buySellRepository;
            _logger = logger;
            _publicationRepository = publicationRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Crea una nueva oferta laboral o de voluntariado
        /// </summary>
        public async Task<GenericResponse<string>> CreateOfferAsync(
            CreateOfferDTO offerDTO,
            GeneralUser currentUser
        )
        {
            if (
                currentUser.UserType != UserType.Empresa
                && currentUser.UserType != UserType.Particular
                && currentUser.UserType != UserType.Administrador
            )
            {
                throw new UnauthorizedAccessException(
                    "Solo usuarios tipo Empresa o Particular pueden crear ofertas."
                );
            }
            // ... (Validaciones de fechas - Mantener tal cual)
            if (offerDTO.EndDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "La fecha de finalización (EndDate) debe ser en el futuro."
                );
            }
            if (offerDTO.DeadlineDate >= offerDTO.EndDate)
            {
                throw new InvalidOperationException(
                    "La fecha límite de postulación (DeadlineDate) debe ser anterior a la fecha de finalización de la oferta."
                );
            }

            // INICIA LA LÓGICA DE PERSISTENCIA DIRECTAMENTE
            var offer = new Offer
            {
                Title = offerDTO.Title,
                Description = offerDTO.Description,
                PublicationDate = DateTime.UtcNow,
                EndDate = offerDTO.EndDate,
                DeadlineDate = offerDTO.DeadlineDate,
                Remuneration = (int)offerDTO.Remuneration,
                OfferType = offerDTO.OfferType,
                Location = offerDTO.Location,
                Requirements = offerDTO.Requirements,
                ContactInfo = offerDTO.ContactInfo,
                IsCvRequired = offerDTO.IsCvRequired,
                UserId = currentUser.Id,
                User = currentUser,
                Type = Types.Offer,
                statusValidation = StatusValidation.InProcess,
                IsActive = false,
            };

            // Si _offerRepository.CreateOfferAsync falla, lanzará una excepción
            // (gracias a que BuySellRepository.cs y OfferRepository.cs la relanzan)
            // y esta será capturada por el controlador como un 500.
            var createdOffer = await _offerRepository.CreateOfferAsync(offer);

            _logger.LogInformation(
                "Oferta creada exitosamente. ID: {OfferId}, Título: {Title}, Usuario: {UserId}",
                createdOffer.Id,
                createdOffer.Title,
                currentUser.Id
            );

            return new GenericResponse<string>(
                "Oferta creada exitosamente",
                $"Oferta ID: {createdOffer.Id}"
            );
        }

        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        public async Task<GenericResponse<string>> CreateBuySellAsync(
            CreateBuySellDTO buySellDTO,
            GeneralUser currentUser
        )
        {
            // Validaciones de negocio (si fallan, lanzan excepción que el controller manejará como 403)
            if (
                currentUser.UserType != UserType.Empresa
                && currentUser.UserType != UserType.Particular
                && currentUser.UserType != UserType.Administrador
            )
            {
                throw new UnauthorizedAccessException(
                    "Solo usuarios tipo Empresa o Particular pueden crear publicaciones de compra/venta."
                );
            }

            // El try-catch de DB es eliminado para que los errores se propaguen al controlador
            var buySell = new BuySell
            {
                Title = buySellDTO.Title,
                Description = buySellDTO.Description,
                UserId = currentUser.Id,
                User = currentUser,
                Type = Types.BuySell,
                Price = buySellDTO.Price,
                Category = buySellDTO.Category,
                Location = buySellDTO.Location,
                ContactInfo = buySellDTO.ContactInfo,
                PublicationDate = DateTime.UtcNow,
                statusValidation = StatusValidation.InProcess,
                IsActive = false,
            };

            // La falla REAL (DB) ocurre dentro de esta llamada
            var createdBuySell = await _buySellRepository.CreateBuySellAsync(buySell);

            _logger.LogInformation(
                "Publicación de compra/venta creada exitosamente. ID: {BuySellId}, Título: {Title}, Usuario: {UserId}",
                createdBuySell.Id,
                createdBuySell.Title,
                currentUser.Id
            );

            return new GenericResponse<string>(
                "Publicación de compra/venta creada exitosamente",
                $"Publicación ID: {createdBuySell.Id}"
            );
        }

        public async Task<IEnumerable<PublicationsDTO>> GetMyPublishedPublicationsAsync(
            string userId
        )
        {
            // 1. Llama al Repositorio para obtener los datos de la BD
            var publications = await _publicationRepository.GetPublishedPublicationsByUserIdAsync(
                userId
            );

            // 2. Mapea las entidades a DTOs
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                IdPublication = p.Id,
                Title = p.Title,
                PublicationDate = p.PublicationDate,
                statusValidation = p.statusValidation,
                UserId = p.UserId,
                Images = p.Images,
                types = p.Type,
                IsActive = p.IsActive,
            });

            return publicationsDto;
        }

        public async Task<IEnumerable<PublicationsDTO>> GetMyRejectedPublicationsAsync(
            string userId
        )
        {
            // 1. Llama al repositorio
            var publications = await _publicationRepository.GetRejectedPublicationsByUserIdAsync(
                userId
            );
            // 2. Mapea y devuelve el DTO
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                IdPublication = p.Id,
                Title = p.Title,
                PublicationDate = p.PublicationDate,
                statusValidation = p.statusValidation,
                UserId = p.UserId,
                Images = p.Images,
                types = p.Type,
                IsActive = p.IsActive,
            });

            return publicationsDto;
        }

        // --- IMPLEMENTACIÓN PENDING ("InProcess") ---
        public async Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId)
        {
            // 1. Llama al repositorio
            var publications = await _publicationRepository.GetPendingPublicationsByUserIdAsync(
                userId
            );
            // 2. Mapea y devuelve el DTO
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                IdPublication = p.Id,
                Title = p.Title,
                PublicationDate = p.PublicationDate,
                statusValidation = p.statusValidation,
                UserId = p.UserId,
                Images = p.Images,
                types = p.Type,
                IsActive = p.IsActive,
            });

            return publicationsDto;
        }
    }
}
