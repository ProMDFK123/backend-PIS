using bolsafe_ucn.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Application.DTOs.AuthDTOs;
using bolsafeucn_back.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using bolsafeucn_back.src.Application.DTOs.UserDTOs;
using bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using CloudinaryDotNet;
using Mapster;
using Serilog;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;
        private readonly IDocumentStorageProvider _documentService;
        private readonly IVerificationCodeRepository _verificationCodeRepository;

        public UserService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IFileRepository fileRepository,
            IVerificationCodeRepository verificationCodeRepository,
            IEmailService emailService,
            ITokenService tokenService,
            IFileService fileService,
            IDocumentStorageProvider documentService
        )
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _fileRepository = fileRepository;
            _emailService = emailService;
            _verificationCodeRepository = verificationCodeRepository;
            _tokenService = tokenService;
            _fileService = fileService;
            _documentService = documentService;
        }

        /// <summary>
        /// Registra un nuevo estudiante en el sistema.
        /// </summary>
        /// <param name="registerStudentDTO">DTO con la información del estudiante</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterStudentAsync(
            RegisterStudentDTO registerStudentDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Iniciando registro de estudiante con email: {Email}",
                registerStudentDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerStudentDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro con email duplicado: {Email}",
                    registerStudentDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerStudentDTO.Rut);
            if (registrado)
            {
                Log.Warning("Intento de registro con RUT duplicado: {Rut}", registerStudentDTO.Rut);
                throw new InvalidOperationException("El RUT ya está en uso.");
            }

            var user = registerStudentDTO.Adapt<GeneralUser>();
            user.PhoneNumber = NormalizePhoneNumber(registerStudentDTO.PhoneNumber);

            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
                ImageType = UserImageType.Banner
            };
            var banner = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultBannerImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultBannerImagePublicId")!,
                ImageType = UserImageType.Perfil
            };
            await _fileRepository.CreateUserImageAsync(profile);
            await _fileRepository.CreateUserImageAsync(banner);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            user.ProfileBanner = banner;
            user.ProfileBannerId = banner.Id;

            var result = await _userRepository.CreateUserAsync(
                user,
                registerStudentDTO.Password,
                "Applicant"
            );
            if (result == false)
            {
                Log.Error(
                    "Error al crear usuario estudiante con email: {Email}",
                    registerStudentDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }


            var student = registerStudentDTO.Adapt<Student>();
            student.GeneralUserId = user.Id;
            result = await _userRepository.CreateStudentAsync(student);
            if (!result)
            {
                Log.Error("Error al crear perfil de estudiante para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al crear el estudiante.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, newCode.Code);
            if (emailResult)
            {
                Log.Information(
                "Estudiante registrado exitosamente con ID: {UserId}, Email: {Email}",
                user.Id,
                user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra un nuevo usuario particular en el sistema.
        /// </summary>
        /// <param name="registerIndividualDTO">Dto de registro del usuario particular</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterIndividualAsync(
            RegisterIndividualDTO registerIndividualDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Iniciando registro de particular con email: {Email}",
                registerIndividualDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerIndividualDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro particular con email duplicado: {Email}",
                    registerIndividualDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerIndividualDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro particular con RUT duplicado: {Rut}",
                    registerIndividualDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerIndividualDTO.Adapt<GeneralUser>();
            user.PhoneNumber = NormalizePhoneNumber(registerIndividualDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
                ImageType = UserImageType.Banner
            };
            var banner = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultBannerImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultBannerImagePublicId")!,
                ImageType = UserImageType.Perfil
            };
            await _fileRepository.CreateUserImageAsync(profile);
            await _fileRepository.CreateUserImageAsync(banner);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            user.ProfileBanner = banner;
            user.ProfileBannerId = banner.Id;
            var result = await _userRepository.CreateUserAsync(
                user,
                registerIndividualDTO.Password,
                "Offerent"
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario particular con email: {Email}",
                    registerIndividualDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            var individual = registerIndividualDTO.Adapt<Individual>();
            individual.GeneralUserId = user.Id;
            result = await _userRepository.CreateIndividualAsync(individual);
            if (!result)
            {
                Log.Error("Error al crear perfil de particular para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al crear el particular.");
            }
            //Envio email de verificacion

            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, newCode.Code);
            if (emailResult)
            {
                Log.Information(
                    "Particular registrado exitosamente con ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra una nueva empresa en el sistema.
        /// </summary>
        /// <param name="registerCompanyDTO">Dto de registro de la empresa</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterCompanyAsync(
            RegisterCompanyDTO registerCompanyDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Iniciando registro de empresa con email: {Email}",
                registerCompanyDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerCompanyDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de empresa con email duplicado: {Email}",
                    registerCompanyDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerCompanyDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de empresa con RUT duplicado: {Rut}",
                    registerCompanyDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerCompanyDTO.Adapt<GeneralUser>();
            user.PhoneNumber = NormalizePhoneNumber(registerCompanyDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
                ImageType = UserImageType.Banner
            };
            var banner = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultBannerImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultBannerImagePublicId")!,
                ImageType = UserImageType.Perfil
            };
            await _fileRepository.CreateUserImageAsync(profile);
            await _fileRepository.CreateUserImageAsync(banner);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            user.ProfileBanner = banner;
            user.ProfileBannerId = banner.Id;
            var result = await _userRepository.CreateUserAsync(
                user,
                registerCompanyDTO.Password,
                "Offerent"
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario empresa con email: {Email}",
                    registerCompanyDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            var company = registerCompanyDTO.Adapt<Company>();
            company.GeneralUserId = user.Id;
            result = await _userRepository.CreateCompanyAsync(company);
            if (!result)
            {
                Log.Error("Error al crear perfil de empresa para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al crear la empresa.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, newCode.Code);
            if (emailResult)
            {
                Log.Information(
                    "Empresa registrada exitosamente con ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra un nuevo administrador en el sistema.
        /// </summary>
        /// <param name="registerAdminDTO">Dto de registro del administrador</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterAdminAsync(
            RegisterAdminDTO registerAdminDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Iniciando registro de admin con email: {Email}, SuperAdmin: {SuperAdmin}",
                registerAdminDTO.Email,
                registerAdminDTO.SuperAdmin
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerAdminDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de admin con email duplicado: {Email}",
                    registerAdminDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerAdminDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de admin con RUT duplicado: {Rut}",
                    registerAdminDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerAdminDTO.Adapt<GeneralUser>();
            user.PhoneNumber = NormalizePhoneNumber(registerAdminDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
                ImageType = UserImageType.Banner
            };
            var banner = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultBannerImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultBannerImagePublicId")!,
                ImageType = UserImageType.Perfil
            };
            await _fileRepository.CreateUserImageAsync(profile);
            await _fileRepository.CreateUserImageAsync(banner);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            user.ProfileBanner = banner;
            user.ProfileBannerId = banner.Id;

            string role = "Admin";
            if (registerAdminDTO.SuperAdmin)
            {
                role = "SuperAdmin";
            }
            var result = await _userRepository.CreateUserAsync(
                user,
                registerAdminDTO.Password,
                role
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario admin con email: {Email}",
                    registerAdminDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            var admin = registerAdminDTO.Adapt<Admin>();
            admin.GeneralUserId = user.Id;
            result = await _userRepository.CreateAdminAsync(admin, registerAdminDTO.SuperAdmin);
            if (!result)
            {
                Log.Error("Error al crear perfil de admin para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al crear el administrador.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, newCode.Code);
            if (emailResult)
            {
                Log.Information(
                    "Admin registrado exitosamente con ID: {UserId}, Email: {Email}, Rol: {Role}",
                    user.Id,
                    user.Email,
                    role
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Verifica el correo electrónico de un usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">Dto de verificación del correo electrónico</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> VerifyEmailAsync(
            VerifyEmailDTO verifyEmailDTO,
            HttpContext httpContext
        )
        {
            Log.Information("Intentando verificar email: {Email}", verifyEmailDTO.Email);
            var user = await _userRepository.GetByEmailAsync(verifyEmailDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de verificación para email no existente: {Email}",
                    verifyEmailDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (user.EmailConfirmed)
            {
                Log.Information("Email ya verificado: {Email}", verifyEmailDTO.Email);
                return "El correo electrónico ya ha sido verificado.";
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            CodeType type = CodeType.EmailConfirmation;

            var verificationCode = testing
                ? new VerificationCode
                {
                    Code = _configuration.GetValue<string>("Testing:FixedVerificationCode") ?? "000000",
                    CodeType = type,
                    GeneralUserId = user.Id,
                    Expiration = DateTime.UtcNow.AddHours(1)
                }
                : await _verificationCodeRepository.GetByLatestUserIdAsync(
                    user.Id,
                    type
                );

            if (
                verificationCode.Code != verifyEmailDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.Expiration
            )
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    type
                );
                Log.Warning(
                    "Intento de verificación fallido para usuario ID: {UserId}, Intentos: {Attempts}",
                    user.Id,
                    attempsCountUpdated
                );

                if (attempsCountUpdated >= 5)
                {
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        type
                    );
                    if (codeDeleteResult)
                    {
                        bool userDeleteResult = await _userRepository.DeleteAsync(user.Id);
                        if (userDeleteResult)
                        {
                            Log.Warning(
                                "Usuario eliminado por exceder intentos de verificación. Email: {Email}, ID: {UserId}",
                                user.Email,
                                user.Id
                            );
                            throw new Exception(
                                "Se ha alcanzado el límite de intentos. El usuario ha sido eliminado."
                            );
                        }
                    }
                }
                if (DateTime.UtcNow >= verificationCode.Expiration)
                {
                    Log.Warning(
                        "Código de verificación expirado para usuario ID: {UserId}",
                        user.Id
                    );
                    throw new Exception("El código de verificación ha expirado.");
                }
                else
                {
                    throw new Exception(
                        $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                    );
                }
            }
            bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
            if (emailConfirmed)
            {
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                    user.Id,
                    type
                );
                if (codeDeleteResult)
                {
                    Log.Information(
                        "Email verificado exitosamente para usuario ID: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email
                    );
                    var emailResult = await _emailService.SendWelcomeEmailAsync(user.Email!);
                    if (emailResult)
                    {
                        Log.Information(
                            "Email de bienvenida enviado exitosamente a: {Email}",
                            user.Email
                        );
                        return "!Ya puedes iniciar sesión!";
                    }
                    else
                    {
                        Log.Error(
                            "Error al enviar email de bienvenida a: {Email}",
                            user.Email
                        );
                        return "Correo verificado, pero hubo un error al enviar el email de bienvenida.";
                    }
                }
                Log.Error(
                    "Error al eliminar código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al confirmar el correo electrónico.");
            }
            Log.Error("Error al confirmar email para usuario ID: {UserId}", user.Id);
            throw new Exception("Error al verificar el correo electrónico.");
        }

        /// <summary>
        /// Reenvia el mensaje de con el codigo de verificacion.
        /// </summary>
        /// <param name="resendVerificationDTO">Dto con los datos del usuatio</param>
        /// <param name="httpContext">Contecto Http</param>
        /// <returns>Mensaje de exito o error</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> ResendVerificationEmailAsync(
            ResendVerificationDTO resendVerificationDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Reenviando código de verificación al email: {Email}",
                resendVerificationDTO.Email
            );

            var user = await _userRepository.GetByEmailAsync(resendVerificationDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de reenvío de verificación para email no existente: {Email}",
                    resendVerificationDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (user.EmailConfirmed)
            {
                Log.Information("Email ya verificado: {Email}", resendVerificationDTO.Email);
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }
            var existingCode = await _verificationCodeRepository.GetByLatestUserIdAsync(
                user.Id,
                CodeType.EmailConfirmation
            );
            bool emailResult;
            if (existingCode != null && DateTime.UtcNow < existingCode.Expiration)
            {
                Log.Information(
                    "Código de verificación aún válido para usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, existingCode.Code);
                if (!emailResult)
                {
                    Log.Error(
                        "Error al reenviar código de verificación para usuario ID: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email
                    );
                    throw new Exception("Error al enviar el correo de verificación.");
                }
                return "El código de verificación anterior aún es válido. Por favor, revisa tu correo electrónico.";
            }
            //Generar nuevo código
            string newCode = new Random().Next(100000, 999999).ToString();
            VerificationCode newVerificationCode = new VerificationCode
            {
                Code = newCode,
                CodeType = CodeType.EmailConfirmation,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var createdCode = await _verificationCodeRepository.CreateCodeAsync(
                newVerificationCode
            );
            if (createdCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            emailResult = await _emailService.SendVerificationEmailAsync(user.Email!, createdCode.Code);
            if (!emailResult)
            {
                Log.Error(
                    "Error al enviar código de verificación para usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                throw new Exception("Error al enviar el correo de verificación.");
            }
            return "Código de verificación reenviado exitosamente.";
        }

        /// <summary>
        /// Inicia sesión en el sistema.
        /// </summary>
        /// <param name="loginDTO">Dto de inicio de sesión</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Token de acceso</returns>
        public async Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
        {
            Log.Information("Intento de login para email: {Email}", loginDTO.Email);

            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                Log.Warning("Intento de login con email no registrado: {Email}", loginDTO.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de login con email no verificado: {Email}, UserId: {UserId}",
                    loginDTO.Email,
                    user.Id
                );
                throw new UnauthorizedAccessException(
                    "Por favor, verifica tu correo electrónico antes de iniciar sesión."
                );
            }
            var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
            if (!result)
            {
                Log.Warning(
                    "Intento de login con contraseña incorrecta para usuario: {Email}, UserId: {UserId}",
                    loginDTO.Email,
                    user.Id
                );
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            var role = await _userRepository.GetRoleAsync(user);
            Log.Information(
                "Login exitoso para usuario: {Email}, UserId: {UserId}, Role: {Role}",
                loginDTO.Email,
                user.Id,
                role
            );
            return _tokenService.CreateToken(user, role, loginDTO.RememberMe);
        }

        /// <summary>
        /// Envía un código de verificación para el reseteo de contraseña al correo electrónico del usuario.
        /// </summary>
        /// <param name="requestResetPasswordCodeDTO">Dto que contiene el email para enviar el código de verificación</param>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <returns>Mensaje indicando el resultado del envío del código de verificación</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> SendResetPasswordVerificationCodeEmailAsync(
            RequestResetPasswordCodeDTO requestResetPasswordCodeDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Enviando código de verificación de reseteo de contraseña al email: {Email}",
                requestResetPasswordCodeDTO.Email
            );
            var user = await _userRepository.GetByEmailAsync(requestResetPasswordCodeDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de reseteo de contraseña para email no registrado: {Email}",
                    requestResetPasswordCodeDTO.Email
                );
                throw new KeyNotFoundException("El usuario no existe no esta registrado.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de reseteo de contraseña para email no confirmado: {Email}",
                    requestResetPasswordCodeDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico no ha sido confirmado.");
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            string code = new Random().Next(100000, 999999).ToString();

            VerificationCode verificationCode = new VerificationCode
            {
                Code = testing
                    ? _configuration.GetValue<string>("Testing:FixedVerificationCode")!
                    : code,
                CodeType = CodeType.PasswordReset,
                GeneralUserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación de reseteo de contraseña para usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }
            var emailResult = await _emailService.SendResetPasswordVerificationEmailAsync(user.Email!, newCode.Code);
            if (emailResult)
            {
                Log.Information(
                    "Código de verificación de reseteo de contraseña enviado exitosamente para usuario ID:{UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Correo de reseteo de contraseña enviado exitosamente.";
            }
            return "El codigo fue creado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Verifica el codigo de reseteo de contraseña ingresado.
        /// </summary>
        /// <param name="verifyResetPasswordCodeDTO">Dto con los el codigo y contraseña</param>
        /// <param name="httpContext">Contexto Http</param>
        /// <returns>Mensaje de exito o error</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> VerifyResetPasswordCodeAsync(
            VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO,
            HttpContext httpContext
        )
        {
            Log.Information(
                "Verificando código de reseteo de contraseña para email: {Email}",
                verifyResetPasswordCodeDTO.Email
            );
            var user = await _userRepository.GetByEmailAsync(verifyResetPasswordCodeDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de verificación de código de reseteo para email no registrado: {Email}",
                    verifyResetPasswordCodeDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de verificación de código de reseteo para email no confirmado: {Email}",
                    verifyResetPasswordCodeDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico no ha sido confirmado.");
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            var verificationCode = testing
                ? new VerificationCode
                {
                    Code = _configuration.GetValue<string>("Testing:FixedVerificationCode") ?? "000000",
                    CodeType = CodeType.PasswordReset,
                    GeneralUserId = user.Id,
                    Expiration = DateTime.UtcNow.AddHours(1)
                }
                : await _verificationCodeRepository.GetByLatestUserIdAsync(
                    user.Id,
                    CodeType.PasswordReset
                );
            if (
                verificationCode.Code != verifyResetPasswordCodeDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.Expiration
            )
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    verificationCode.CodeType
                );
                Log.Warning(
                    "Intento de verificación fallido para usuario ID: {UserId}, Intentos: {Attempts}",
                    user.Id,
                    attempsCountUpdated
                );

                if (attempsCountUpdated >= 5)
                {
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        verificationCode.CodeType
                    );
                    if (codeDeleteResult)
                    {
                        Log.Warning(
                            "Código de verificación eliminado por exceder intentos. Email: {Email}, ID: {UserId}",
                            user.Email,
                            user.Id
                        );
                        throw new Exception(
                            "Se ha alcanzado el límite de intentos. El código de verificación ha sido eliminado."
                        );
                    }
                }
                if (DateTime.UtcNow >= verificationCode.Expiration)
                {
                    Log.Warning(
                        "Código de verificación expirado para usuario ID: {UserId}",
                        user.Id
                    );
                    throw new Exception("El código de verificación ha expirado.");
                }
                else
                {
                    throw new Exception(
                        $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                    );
                }
            }
            Log.Information(
                "Código de verificación de reseteo de contraseña válido para usuario ID: {UserId}",
                user.Id
            );
            var newPasswordResult = await _userRepository.UpdatePasswordAsync(
                user,
                verifyResetPasswordCodeDTO.Password
            );
            if (!newPasswordResult)
            {
                Log.Error("Error al actualizar la contraseña para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al actualizar la contraseña.");
            }
            return "Contraseña actualizada exitosamente.";
        }

        #region User Profiles

        /// <summary>
        /// Obtiene el perfil de un estudiante por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Perfil del usuario</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<IGetUserProfileDTO> GetUserProfileByIdAsync(int userId, UserType userType)
        {
            Log.Information($"Buscando usuario con la ID: {userId}");
            GeneralUser? user = await _userRepository.
                GetUntrackedWithTypeAsync(userId, userType)
                ?? throw new KeyNotFoundException("No existe usuario con ese ID");

            Log.Information("Buscando detalles relevantes");
            return userType switch
            {
                UserType.Estudiante => user.Adapt<GetStudentProfileDTO>(),
                UserType.Particular => user.Adapt<GetIndividualProfileDTO>(),
                UserType.Empresa => user.Adapt<GetCompanyProfileDTO>(),
                _ => user.Adapt<GetAdminProfileDTO>(), //UserType.Administrador
            };
        }

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="updateParamsDTO">Parámetros de actualización</param>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Mensaje de exito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> UpdateUserProfileByIdAsync(IUpdateParamsDTO updateParamsDTO, int userId, UserType userType)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            GeneralUser? user = await _userRepository.
                GetTrackedWithTypeAsync(userId, userType)
                ?? throw new KeyNotFoundException("No existe usuario con ese ID");
            updateParamsDTO.ApplyTo(user);

            // Validar que los campos requeridos por el sistema estén presentes después de aplicar los cambios
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Rut))
            {
                Log.Error("El usuario actualizado no tiene todos los campos requeridos. UserId: {UserId}", userId);
                throw new Exception("El usuario actualizado no tiene todos los campos requeridos.");
            }

            var result = await _userRepository.UpdateAsync(user);
            if (!result)
            {
                Log.Error("Error al actualizar los datos del usuario con ID: {UserId}", userId);
                throw new Exception("Error al actualizar los datos del usuario");
            }
            return "Datos del usuario actualizados correctamente";
        }

        public async Task<GetPhotoDTO> GetUserProfilePhotoByIdAsync(int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            GeneralUser? user = await _userRepository.
                GetUntrackedWithTypeAsync(userId, null) // El tipo de usuario no importa aquí
                ?? throw new KeyNotFoundException("No existe usuario con ese ID");

            var photoDto = new GetPhotoDTO
            {
                PhotoUrl = user.ProfilePhoto?.Url ?? string.Empty,
            };

            return photoDto;
        }

        /// <summary>
        /// Actualiza la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updatePhotoDTO">Parámetros para actualizar la foto de perfil del usuario.</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de éxito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> UpdateUserProfilePhotoByIdAsync(UpdatePhotoDTO updatePhotoDTO, int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            GeneralUser? user = await _userRepository.
                GetTrackedWithTypeAsync(userId, null) // El tipo de usuario no importa aquí
                ?? throw new KeyNotFoundException("No existe usuario con ese ID");

            await _fileService.UploadUserImageAsync(
                updatePhotoDTO.Photo,
                user,
                UserImageType.Perfil);

            var result = await _userRepository.UpdateAsync(user);
            if (!result)
            {
                Log.Error("Error al actualizar la foto de perfil del usuario con ID: {UserId}", userId);
                throw new Exception("Error al actualizar la foto de perfil del usuario");
            }
            return "Foto de perfil del usuario actualizada correctamente";
        }

        /// <summary>
        /// Cambia la contraseña de un usuario por su ID.
        /// </summary>
        /// <param name="changeUserPasswordDTO">Parámetros para cambiar la contraseña del usuario.</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de éxito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> ChangeUserPasswordById(ChangeUserPasswordDTO changeUserPasswordDTO, int userId)
        {
            Log.Information("Buscando usuario con la Id: {UserId}", userId.ToString());
            GeneralUser? user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("No existe usuario con ese Id");
            var passwordValid = await _userRepository.CheckPasswordAsync(user, changeUserPasswordDTO.CurrentPassword);
            if (!passwordValid)
            {
                Log.Warning("Intento de cambio de contraseña fallido para usuario ID: {UserId} debido a contraseña actual incorrecta", userId.ToString());
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
            }
            var result = await _userRepository.UpdatePasswordAsync(user, changeUserPasswordDTO.NewPassword);
            if (!result)
            {
                Log.Error("Error al actualizar la contraseña para usuario ID: {UserId}", userId.ToString());
                throw new Exception("Error al actualizar la contraseña.");
            }
            return "Contraseña actualizada exitosamente.";
        }

        #endregion
        #region Documents Management
        public async Task<string> UploadCVByIdAsync(UploadCVDTO uploadCVDTO, int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            GeneralUser? user = await _userRepository.
                GetTrackedWithTypeAsync(userId, UserType.Estudiante)
                ?? throw new KeyNotFoundException("No existe estudiante con ese ID");
            if (user.CV != null)
            {
                Log.Information("El usuario con ID: {UserId} ya tiene un CV, se reemplazará el existente.", user.Id);
                var deleteResult = await _documentService.DeleteCVAsync(user);
                if (!deleteResult)
                {
                    Log.Error("Error al eliminar el CV existente del estudiante con ID: {UserId}", user.Id);
                    throw new Exception("Error al eliminar el CV existente del estudiante");
                }
            }
            var uploadResult = await _documentService.UploadCVAsync(
                uploadCVDTO.CVFile,
                user
            );
            if (!uploadResult)
            {
                Log.Error("Error al subir el CV del estudiante con ID: {UserId}", user.Id);
                throw new Exception("Error al subir el CV del estudiante");
            }
           
            return "CV del usuario actualizado correctamente";
        }
        public async Task<GetCVDTO> DownloadCVByIdAsync(int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            GeneralUser? user = await _userRepository.
                GetTrackedWithTypeAsync(userId, UserType.Estudiante)
                ?? throw new KeyNotFoundException("No existe estudiante con ese ID");
            if (user.CV == null)
            {
                Log.Warning("El usuario con ID: {UserId} no tiene un CV para descargar.", user.Id);
                throw new KeyNotFoundException("El usuario no tiene un CV para descargar");
            }
            return new GetCVDTO
            {
                Url = user.CV.Url,
                OriginalFileName = user.CV.OriginalFileName,
                FileSizeBytes = user.CV.FileSizeBytes,
                UploadDate = user.CV.CreatedAt,
            };
        }
        public async Task<string> DeleteCVByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }
        #endregion

        /*public async Task<IEnumerable<GeneralUser>> GetUsuariosAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<GeneralUser?> GetUsuarioAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<GeneralUser> CrearUsuarioAsync(UsuarioDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }*/

        #region Helper Functions
        /// <summary>
        /// Funcion helper para normalizar el numero de telefono.
        /// </summary>
        /// <param name="phoneNumber">Numero de telefono del usuario</param>
        /// <returns>Numero de telefono normalizado</returns>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return "+56" + digits;
        }

        #endregion
    }
}
