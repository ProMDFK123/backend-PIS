using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        Task<Whitelist> AddToWhitelistAsync(Whitelist token);
        Task<bool> RemoveAllFromWhitelistByUserIdAsync(int userId);
        Task<bool> ExistsByUserIdAsync(int userId);
        Task<bool> IsTokenWhitelistedAsync(int userId, string token);
    }
}