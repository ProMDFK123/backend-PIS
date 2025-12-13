using bolsafeucn_back.src.Domain.Models;

namespace bolsafe_ucn.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(GeneralUser user, string roleName, bool rememberMe);
    }
}
