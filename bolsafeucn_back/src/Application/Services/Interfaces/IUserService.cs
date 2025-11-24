using bolsafeucn_back.src.Application.DTOs.AuthDTOs;
using bolsafeucn_back.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using bolsafeucn_back.src.Application.DTOs.UserDTOs;
using bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        /*
        Task<IEnumerable<GeneralUser>> GetUsuariosAsync();
        Task<GeneralUser?> GetUsuarioAsync(int id);
        Task<GeneralUser> CrearUsuarioAsync(UsuarioDto dto);
        Task<bool> EliminarUsuarioAsync(int id);
        */
        #region Registro de usuarios
        Task<string> RegisterStudentAsync(
            RegisterStudentDTO registerStudentDTO,
            HttpContext httpContext
        );
        Task<string> RegisterIndividualAsync(
            RegisterIndividualDTO registerIndividualDTO,
            HttpContext httpContext
        );
        Task<string> RegisterCompanyAsync(
            RegisterCompanyDTO registerCompanyDTO,
            HttpContext httpContext
        );
        Task<string> RegisterAdminAsync(RegisterAdminDTO registerAdminDTO, HttpContext httpContext);
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO, HttpContext httpContext);
        Task<string> ResendVerificationEmailAsync(
            ResendVerificationDTO resendVerificationDTO,
            HttpContext httpContext
        #endregion
        #region Login and Password Management
        );
        Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);
        Task<string> SendResetPasswordVerificationCodeEmailAsync(
            RequestResetPasswordCodeDTO requestResetPasswordCodeDTO,
            HttpContext httpContext
        );
        Task<string> VerifyResetPasswordCodeAsync(
            VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO,
            HttpContext httpContext
        );
        Task<string> ChangeUserPasswordById(ChangeUserPasswordDTO changeUserPasswordDTO, int userId);
        #endregion
        #region Profile Management
        Task<IGetUserProfileDTO> GetUserProfileByIdAsync(int userId, UserType userType);
        Task<string> UpdateUserProfileByIdAsync(IUpdateParamsDTO updateParamsDTO, int userId, UserType userType);
        #endregion 
    }
}
