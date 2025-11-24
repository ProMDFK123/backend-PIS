using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<GeneralUser?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByRutAsync(string rut);
        Task<bool> CreateUserAsync(GeneralUser user, string password, string role);
        Task<bool> CreateStudentAsync(Student student);
        Task<bool> CreateIndividualAsync(Individual individual);
        Task<bool> CreateCompanyAsync(Company company);
        Task<bool> CreateAdminAsync(Admin admin, bool superAdmin);
        Task<bool> CheckPasswordAsync(GeneralUser user, string password);
        Task<bool> UpdateAsync(GeneralUser user);
        Task<bool> UpdatePasswordAsync(GeneralUser user, string newPassword);
        Task<string> GetRoleAsync(GeneralUser user);
        Task<GeneralUser> GetGeneralUserByIdAsync(int id);
        Task<IEnumerable<GeneralUser>> GetAllAsync();
        Task<bool> ConfirmEmailAsync(string email);
        Task<GeneralUser?> GetByIdAsync(int id);
        Task<GeneralUser?> GetByIdWithRelationsAsync(int id);
        Task<GeneralUser?> GetUntrackedWithTypeAsync(int id, UserType userType);
        Task<GeneralUser?> GetTrackedWithTypeAsync(int id, UserType userType);  
        Task<GeneralUser> AddAsync(GeneralUser usuario);
        Task<bool> DeleteAsync(int id);
    }
}
