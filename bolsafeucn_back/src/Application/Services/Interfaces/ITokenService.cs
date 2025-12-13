using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(GeneralUser user, string roleName, bool rememberMe);
        Task<bool> AddToWhitelistAsync(Whitelist token);
        Task<bool> RevokeAllActiveTokensAsync(int userId);
    }
}
