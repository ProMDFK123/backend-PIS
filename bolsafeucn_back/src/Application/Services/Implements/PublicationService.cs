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
        private readonly IReviewService _reviewService;

        public PublicationService(
            IOfferRepository offerRepository,
            IBuySellRepository buySellRepository,
            ILogger<PublicationService> logger,
            IPublicationRepository publicationRepository,
            IMapper mapper,
            IReviewService reviewService
        )
        {
            _offerRepository = offerRepository;
            _buySellRepository = buySellRepository;
            _logger = logger;
            _publicationRepository = publicationRepository;
            _mapper = mapper;
            _reviewService = reviewService;
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
                && currentUser.UserType != UserType.Estudiante
            )
            {
                throw new UnauthorizedAccessException(
                    "Solo usuarios tipo Empresa, Particular, Administrador o Estudiante registrado pueden crear ofertas."
                );
            }
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
            // Validar que el usuario no tenga más de 3 reseñas pendientes
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(
                currentUser.Id
            );
            Console.WriteLine(
                $"Reseñas pendientes para el usuario {currentUser.Id}: {pendingReviewsCount}"
            );
            if (pendingReviewsCount >= 3)
            {
                _logger.LogWarning(
                    "Usuario {UserId} intentó crear publicación de compra/venta con {PendingCount} reseñas pendientes",
                    currentUser.Id,
                    pendingReviewsCount
                );
                throw new InvalidOperationException("No puedes crear una oferta mientras tengas 3 o más reseñas pendientes de revisión.");
            }
            try
            {
                var isAdmin = currentUser.UserType == UserType.Administrador;

                var offer = new Offer
                {
                    Title = offerDTO.Title,
                    Description = offerDTO.Description,
                    PublicationDate = DateTime.UtcNow,
                    EndDate = offerDTO.EndDate.ToUniversalTime(),
                    DeadlineDate = offerDTO.DeadlineDate.ToUniversalTime(),
                    Remuneration = (int)offerDTO.Remuneration,
                    OfferType = offerDTO.OfferType,
                    Location = offerDTO.Location,
                    Requirements = offerDTO.Requirements,
                    ContactInfo = offerDTO.ContactInfo,
                    IsCvRequired = offerDTO.IsCvRequired,
                    UserId = currentUser.Id,
                    User = currentUser,
                    Type = Types.Offer,

                    statusValidation = isAdmin
                        ? StatusValidation.Published
                        : StatusValidation.InProcess,

                    IsActive = isAdmin,
                };

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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error de validación al crear oferta para el usuario {UserId}: {ErrorMessage}",
                    currentUser.Id,
                    ex.Message
                );
                return new GenericResponse<string>($"Error al crear la oferta: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al crear oferta para el usuario {UserId}",
                    currentUser.Id
                );
                return new GenericResponse<string>($"Error al crear la oferta: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Crea una nueva publicación de compra/venta
        /// </summary>
        public async Task<GenericResponse<string>> CreateBuySellAsync(
            CreateBuySellDTO buySellDTO,
            GeneralUser currentUser
        )
        {
            if (
                currentUser.UserType != UserType.Empresa
                && currentUser.UserType != UserType.Particular
                && currentUser.UserType != UserType.Administrador
                && currentUser.UserType != UserType.Estudiante
            )
            {
                throw new UnauthorizedAccessException(
                    "Solo usuarios tipo Empresa, Particular, Administrador o Estudiante registrado pueden crear ofertas."
                );
            }
            // Validar que el usuario no tenga más de 3 reseñas pendientes
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(
                currentUser.Id
            );
            if (pendingReviewsCount >= 3)
            {
                _logger.LogWarning(
                    "Usuario {UserId} intentó crear publicación de compra/venta con {PendingCount} reseñas pendientes",
                    currentUser.Id,
                    pendingReviewsCount
                );
                throw new InvalidOperationException("No puedes crear una publicación de compra/venta mientras tengas 3 o más reseñas pendientes de revisión.");
            }
            try
            {
                var isAdmin = currentUser.UserType == UserType.Administrador;

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

                    statusValidation = isAdmin
                        ? StatusValidation.Published
                        : StatusValidation.InProcess,

                    IsActive = isAdmin,
                };

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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error de validación al crear publicación de compra/venta para el usuario {UserId}: {ErrorMessage}",
                    currentUser.Id,
                    ex.Message
                );
                return new GenericResponse<string>(
                    $"Error al crear la publicación de compra/venta: {ex.Message}",
                    null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al crear publicación de compra/venta para el usuario {UserId}",
                    currentUser.Id
                );
                return new GenericResponse<string>(
                    $"Error al crear la publicación de compra/venta: {ex.Message}",
                    null
                );
            }
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
        // --- IMPLEMENTACIÓN PENDING ("InProcess") CORREGIDA ---
        public async Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId)
        {
            // 1. Llama al repositorio
            var publications = await _publicationRepository.GetPendingPublicationsByUserIdAsync(
                userId
            );

            // 2. Mapea y devuelve el DTO MANUALMENTE (Para asegurar el ID)
            var publicationsDto = publications.Select(p => new PublicationsDTO
            {
                // ✅ AQUÍ ASIGNAMOS EL ID CORRECTAMENTE
                IdPublication = p.Id,

                Title = p.Title,
                Description = p.Description, // ¡No olvides la descripción!
                PublicationDate = p.PublicationDate,
                statusValidation = p.statusValidation,
                UserId = p.UserId,
                Images = p.Images,
                types = p.Type,
                IsActive = p.IsActive,
            });

            return publicationsDto;
        }

        /// <summary>
        /// Maximum number of times a user can appeal a rejection for a single publication.
        /// </summary>
        private const int MAX_APPEALS = 3;

        public PublicationService(IPublicationRepository publicationRepository)
        {
            _publicationRepository = publicationRepository;
        }

        /// <summary>
        /// Approves a publication and makes it visible.
        /// </summary>
        public async Task<GenericResponse<string>> AdminApprovePublicationAsync(int publicationId)
        {
            // 1. Search for publication
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            // 2. Validation: Use Exception to trigger Middleware 404
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 3. Update logic
            publication.statusValidation = StatusValidation.Published;
            publication.IsActive = true;

            await _publicationRepository.UpdateAsync(publication);

            // 4. Return success only
            return new GenericResponse<string>("Publicación aprobada y publicada exitosamente.");
        }

        /// <summary>
        /// Rejects a publication with a reason and hides it.
        /// </summary>
        public async Task<GenericResponse<string>> AdminRejectPublicationAsync(
            int publicationId,
            AdminRejectDto dto
        )
        {
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            publication.statusValidation = StatusValidation.Rejected;
            publication.IsActive = false;
            publication.AdminRejectionReason = dto.Reason;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>("Publicación rechazada. Se ha guardado el motivo.");
        }

        /// <summary>
        /// Allows a user to appeal a rejection if limits allow.
        /// </summary>
        public async Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        )
        {
            var publication = await _publicationRepository.GetByIdAsync(publicationId);

            // 1. Validate existence -> 404 Not Found
            if (publication == null)
                throw new KeyNotFoundException("La publicación no existe.");

            // 2. Validate ownership -> 401 Unauthorized (Based on JobApplicationService pattern)
            if (publication.UserId != userId)
                throw new UnauthorizedAccessException(
                    "No tienes permiso para apelar esta publicación."
                );

            // 3. Validate status -> 409 Conflict (InvalidOperationException maps to 409 in your middleware)
            if (publication.statusValidation != StatusValidation.Rejected)
                throw new InvalidOperationException(
                    "Solo se pueden apelar publicaciones que han sido rechazadas."
                );

            // 4. Validate limits -> 409 Conflict
            if (publication.AppealCount >= MAX_APPEALS)
            {
                throw new InvalidOperationException(
                    $"Has alcanzado el límite máximo de {MAX_APPEALS} apelaciones para esta publicación."
                );
            }

            // 5. Process appeal
            publication.statusValidation = StatusValidation.InProcess;
            publication.UserAppealJustification = dto.Justification;
            publication.AppealCount++;

            await _publicationRepository.UpdateAsync(publication);

            return new GenericResponse<string>(
                $"Apelación enviada exitosamente. Intento {publication.AppealCount} de {MAX_APPEALS}. Un administrador revisará tu caso."
            );
        }
    }
}
