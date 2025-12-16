using bolsafeucn_back.src.Application.DTOs.BaseResponse;
using bolsafeucn_back.src.Application.DTOs.PublicationDTO;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<GenericResponse<string>> CreateOfferAsync(
            CreateOfferDTO publicationDTO,
            GeneralUser currentUser
        );
        Task<GenericResponse<string>> CreateBuySellAsync(
            CreateBuySellDTO publicationDTO,
            GeneralUser currentUser
        );

        /// <summary>
        /// Funcion
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<PublicationsDTO>> GetMyPublishedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyRejectedPublicationsAsync(string userId);
        Task<IEnumerable<PublicationsDTO>> GetMyPendingPublicationsAsync(string userId);

        Task<GenericResponse<string>> AdminApprovePublicationAsync(int publicationId);

        Task<GenericResponse<string>> AdminRejectPublicationAsync(
            int publicationId,
            AdminRejectDto dto
        );
        Task<GenericResponse<string>> AppealPublicationAsync(
            int publicationId,
            int userId,
            UserAppealDto dto
        );
    }
}
