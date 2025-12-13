using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Interfaces
{
    public interface IVerificationCodeRepository
    {
        Task<VerificationCode> CreateCodeAsync(VerificationCode code);
        Task<VerificationCode> UpdateCodeAsync(VerificationCode code);
        Task<VerificationCode> GetByCodeAsync(string code, CodeType codeType);
        Task<VerificationCode> GetByLatestUserIdAsync(int userId, CodeType tipo);
        Task<int> IncreaseAttemptsAsync(int userId, CodeType codeType);
        Task<bool> DeleteByUserIdAsync(int userId, CodeType tipo);
    }
}
