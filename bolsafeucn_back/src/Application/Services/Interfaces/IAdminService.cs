using bolsafeucn_back.src.Application.DTOs.UserDTOs.AdminDTOs;

namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task<bool> ToggleUserBlockedStatusAsync(int adminId, int userId);
        Task<UsersForAdminDTO> GetAllUsersAsync(int adminId, SearchParamsDTO searchParams);
        Task<UserProfileForAdminDTO> GetUserProfileByIdAsync(int adminId, int userId);
    }
}