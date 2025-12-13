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
        /// <summary>
        /// Funcion de registro de estudiante
        /// </summary>
        /// <param name="registerStudentDTO">Datos para registrar un estudiante</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterStudentAsync(
            RegisterStudentDTO registerStudentDTO,
            HttpContext httpContext
        );
        /// <summary>
        /// Funcion de registro de persona particular
        /// </summary>
        /// <param name="registerIndividualDTO">Datos para registrar una persona particular</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterIndividualAsync(
            RegisterIndividualDTO registerIndividualDTO,
            HttpContext httpContext
        );
        /// <summary>
        /// Funcion de registro de empresa
        /// </summary>
        /// <param name="registerCompanyDTO">Datos para registrar una empresa</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterCompanyAsync(
            RegisterCompanyDTO registerCompanyDTO,
            HttpContext httpContext
        );
        /// <summary>
        /// Funcion de registro de administrador
        /// </summary>
        /// <param name="registerAdminDTO">Datos para registrar un administrador</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterAdminAsync(RegisterAdminDTO registerAdminDTO, HttpContext httpContext);
        /// <summary>
        /// Verifica el correo electrónico de un usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">Datos para verificar el correo electrónico</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado de la verificación</returns>
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO, HttpContext httpContext);
        /// <summary>
        /// Reenvía el correo de verificación a un usuario.
        /// </summary>
        /// <param name="resendVerificationDTO">Datos para reenviar el correo de verificación</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del reenvío</returns>
        Task<string> ResendVerificationEmailAsync(
            ResendVerificationDTO resendVerificationDTO,
            HttpContext httpContext
        );
        #endregion
        #region Login and Password Management
        /// <summary>
        /// Inicia sesión de un usuario.
        /// </summary>
        /// <param name="loginDTO">Datos para iniciar sesión</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del inicio de sesión</returns>
        Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);
        /// <summary>
        /// Envía un correo electrónico con un código de verificación para restablecer la contraseña.
        /// </summary>
        /// <param name="requestResetPasswordCodeDTO">Datos para solicitar el código de verificación</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado del envío</returns>
        Task<string> SendResetPasswordVerificationCodeEmailAsync(
            RequestResetPasswordCodeDTO requestResetPasswordCodeDTO,
            HttpContext httpContext
        );
        /// <summary>
        /// Verifica el código de restablecimiento de contraseña.
        /// </summary>
        /// <param name="verifyResetPasswordCodeDTO">Datos para verificar el código de restablecimiento</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de resultado de la verificación</returns>
        Task<string> VerifyResetPasswordCodeAsync(
            VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO,
            HttpContext httpContext
        );
        /// <summary>
        /// Restablece la contraseña de un usuario.
        /// </summary>
        /// <param name="changeUserPasswordDTO">Datos para cambiar la contraseña del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado del cambio de contraseña</returns>
        Task<string> ChangeUserPasswordById(ChangeUserPasswordDTO changeUserPasswordDTO, int userId);
        #endregion
        #region Profile Management
        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Perfil del usuario</returns>
        Task<IGetUserProfileDTO> GetUserProfileByIdAsync(int userId, UserType userType);
        /// <summary>
        /// Actualiza el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updateParamsDTO">Datos para actualizar el perfil del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Mensaje de resultado de la actualización</returns>
        Task<string> UpdateUserProfileByIdAsync(IUpdateParamsDTO updateParamsDTO, int userId, UserType userType);
        /// <summary>
        /// Obtiene la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Foto de perfil del usuario</returns>
        Task<GetPhotoDTO> GetUserProfilePhotoByIdAsync(int userId);
        /// <summary>
        /// Actualiza la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updatePhotoDTO">Datos para actualizar la foto de perfil</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado de la actualización</returns>
        Task<string> UpdateUserProfilePhotoByIdAsync(UpdatePhotoDTO updatePhotoDTO, int userId);
        #endregion 
        #region Documents Management
        /// <summary>
        /// Sube el CV de un usuario por su ID.
        /// </summary>
        /// <param name="uploadCVDTO">Datos para subir el CV</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado de la subida</returns>
        Task<string> UploadCVByIdAsync(UploadCVDTO uploadCVDTO, int userId);
        /// <summary>
        /// Descarga el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Archivo del CV</returns>
        Task<GetCVDTO> DownloadCVByIdAsync(int userId);
        /// <summary>
        /// Elimina el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Indica si la eliminación fue exitosa</returns>
        Task<string> DeleteCVByIdAsync(int userId);
        #endregion
    }
}
