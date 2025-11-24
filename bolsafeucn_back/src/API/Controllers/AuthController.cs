using bolsafeucn_back.src.Application.DTOs.AuthDTOs;
using bolsafeucn_back.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using bolsafeucn_back.src.Application.DTOs.BaseResponse;
using bolsafeucn_back.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace bolsafeucn_back.src.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IUserService _service;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _service = userService;
        }

        [HttpPost("register/student")]
        public async Task<IActionResult> Register([FromBody] RegisterStudentDTO registerStudentDTO)
        {
            Log.Information(
                "Attempting to register new student with email {Email}",
                registerStudentDTO.Email
            );
            var message = await _service.RegisterStudentAsync(registerStudentDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro de estudiante exitoso", message));
        }

        [HttpPost("register/individual")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterIndividualDTO registerIndividualDTO
        )
        {
            Log.Information(
                "Endpoint: POST /api/auth/register/individual - Intentando registrar particular con email: {Email}",
                registerIndividualDTO.Email
            );
            var message = await _service.RegisterIndividualAsync(
                registerIndividualDTO,
                HttpContext
            );
            return Ok(new GenericResponse<string>("Registro de particular exitoso", message));
        }

        [HttpPost("register/company")]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyDTO registerCompanyDTO)
        {
            Log.Information(
                "Endpoint: POST /api/auth/register/company - Intentando registrar empresa con email: {Email}",
                registerCompanyDTO.Email
            );
            var message = await _service.RegisterCompanyAsync(registerCompanyDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro de empresa exitoso", message));
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> Register([FromBody] RegisterAdminDTO registerAdminDTO)
        {
            Log.Information(
                "Endpoint: POST /api/auth/register/admin - Intentando registrar admin con email: {Email}",
                registerAdminDTO.Email
            );
            var message = await _service.RegisterAdminAsync(registerAdminDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro de admin exitoso", message));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            Log.Information(
                "Endpoint: POST /api/auth/verify-email - Intentando verificar email: {Email}",
                verifyEmailDTO.Email
            );
            var message = await _service.VerifyEmailAsync(verifyEmailDTO, HttpContext);
            return Ok(new GenericResponse<string>("Verificación de email exitosa", message));
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail(
            [FromBody] ResendVerificationDTO resendVerificationDTO
        )
        {
            Log.Information(
                "Endpoint: POST /api/auth/resend-verification - Reenviando código de verificación al email: {Email}",
                resendVerificationDTO.Email
            );
            var message = await _service.ResendVerificationEmailAsync(
                resendVerificationDTO,
                HttpContext
            );
            return Ok(new GenericResponse<string>("Código de verificación reenviado", message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            Log.Information(
                "Endpoint: POST /api/auth/login - Intento de login para: {Email}",
                loginDTO.Email
            );
            var token = await _service.LoginAsync(loginDTO, HttpContext);
            return Ok(new GenericResponse<string>("Login exitoso", token));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> SendResetPasswordVerificationCodeEmail(
            [FromBody] RequestResetPasswordCodeDTO requestResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Endpoint: POST /api/auth/reset-password - Intento de reseteo de contraseña para: {Email}",
                requestResetPasswordCodeDTO.Email
            );
            var message = await _service.SendResetPasswordVerificationCodeEmailAsync(
                requestResetPasswordCodeDTO,
                HttpContext
            );
            return Ok(
                new GenericResponse<string>("Correo de reseteo de contraseña enviado", message)
            );
        }

        [HttpPost("reset-password/verify")]
        public async Task<IActionResult> VerifyResetPasswordCode(
            [FromBody] VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Endpoint: POST /api/auth/reset-password/verify - Intento de verificación de código de reseteo de contraseña para: {Email}",
                verifyResetPasswordCodeDTO.Email
            );
            var message = await _service.VerifyResetPasswordCodeAsync(
                verifyResetPasswordCodeDTO,
                HttpContext
            );
            return Ok(
                new GenericResponse<string>(
                    "Verificación de código de reseteo de contraseña exitosa",
                    message
                )
            );
        }

        /*
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _service.GetUsuarioAsync(id);
            if (usuario == null)
                return NotFound();
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UsuarioDto dto)
        {
            var usuario = await _service.CrearUsuarioAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.EliminarUsuarioAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        */
    }
}
