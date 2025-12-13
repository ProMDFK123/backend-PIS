using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using bolsafe_ucn.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecret = _configuration.GetValue<string>("Jwt:Key")!;
        }

        /// <summary>
        /// Crea un token JWT para el usuario dado.
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="roleName">Nombre del rol</param>
        /// <param name="rememberMe">Indica si se debe recordar al usuario</param>
        /// <returns>Token JWT</returns>
        public string CreateToken(GeneralUser user, string roleName, bool rememberMe)
        {
            try
            {
                Log.Information(
                    "Creando token JWT para usuario ID: {UserId}, Email: {Email}, Role: {Role}, RememberMe: {RememberMe}",
                    user.Id,
                    user.Email,
                    roleName,
                    user.UserType.ToString(),
                    rememberMe
                );

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("userName", user.UserName!.ToString()),
                    new Claim("userType", user.UserType.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expirationHours = rememberMe ? 24 : 1;
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(expirationHours),
                    signingCredentials: creds
                );

                Log.Information(
                    "Token JWT creado exitosamente para usuario ID: {UserId}, expira en {Hours} horas",
                    user.Id,
                    expirationHours
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear token JWT para usuario ID: {UserId}", user.Id);
                throw new InvalidOperationException("Error creando el token JWT.", ex);
            }
        }
    }
}
