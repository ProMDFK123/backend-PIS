using bolsafeucn_back.src.Application.DTOs.UserDTOs.AdminDTOs;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public AdminService(IUserRepository userRepository, ITokenService tokenService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        /// <summary>
        /// Alterna el estado de bloqueo de un usuario dado su ID, siempre y cuando el solicitante sea un administrador.
        /// </summary>
        /// <param name="adminId">ID del administrador que realiza la acción.</param>
        /// <param name="userId">ID del usuario cuyo estado de bloqueo se desea alternar.</param>
        /// <returns>El nuevo estado de bloqueo del usuario.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> ToggleUserBlockedStatusAsync(int adminId, int userId)
        {
            //Chequeo de autobloqueo
            if (userId == adminId)
            {
                Log.Warning("Un administrador intentó alternar su propio estado de bloqueo.");
                throw new InvalidOperationException("Un administrador no puede bloquear o desbloquearse a sí mismo.");
            }
            Log.Information($"Administrador con ID {adminId} está intentando alternar el estado de bloqueo del usuario con ID {userId}.");

            // Verificar que el solicitante es un administrador
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                Log.Warning($"No se encontró al administrador con ID {adminId}.");
                throw new KeyNotFoundException("Administrador no encontrado.");
            }
            if (admin.UserType != UserType.Administrador)
            {
                Log.Warning($"El usuario con ID {adminId} no tiene permisos de administrador.");
                throw new UnauthorizedAccessException("El usuario no tiene permisos de administrador.");
            }

            Log.Information($"Buscando al usuario con ID {userId} para alternar su estado de bloqueo.");

            // Obtener el usuario objetivo
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) // Verificar que el usuario existe
            {
                Log.Warning($"No se encontró al usuario con ID {userId}.");
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (user.UserType == UserType.Administrador && admin.Admin!.SuperAdmin) // Prevenir bloqueo de administradores si es que es el ultimo
            {
                var numberOfAdmins = _userRepository.GetNumberOfAdmins();
                if (numberOfAdmins.Result <= 1)
                {
                    Log.Warning("Intento de bloquear al último administrador.");
                    throw new InvalidOperationException("No se puede bloquear al último administrador.");
                }
                Log.Warning($"Intento de alternar el estado de bloqueo del usuario con ID {userId}, que es un administrador.");
                throw new InvalidOperationException("No se puede bloquear o desbloquear a un administrador.");
            }

            user.Banned = !user.Banned; // Alternar el estado de bloqueo

            var toggleResult = await _userRepository.UpdateAsync(user);
            if (toggleResult)
            {
                Log.Information($"El estado de bloqueo del usuario con ID {userId} ha sido alternado a {user.Banned}.");
                if (user.Banned)
                {
                    var revokeResult = await _tokenService.RevokeAllActiveTokensAsync(userId);
                    Log.Information(revokeResult
                        ? $"Tokens activos revocados para el usuario con ID {userId} tras ser bloqueado."
                        : $"El usuario con ID {userId} no tenía tokens activos para revocar tras ser bloqueado.");
                }
                return user.Banned;
            }
            else
            {
                Log.Error($"Error al actualizar el estado de bloqueo del usuario con ID {userId}.");
                throw new Exception("Error al actualizar el estado de bloqueo del usuario.");
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema para un administrador.
        /// </summary>
        /// <param name="adminId">ID del administrador que realiza la solicitud</param>
        /// <returns>DTO con la lista de usuarios</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<UsersForAdminDTO> GetAllUsersAsync(int adminId, SearchParamsDTO searchParams)
        {
            // Verificar que el solicitante es un administrador
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                Log.Warning($"No se encontró al administrador con ID {adminId}.");
                throw new KeyNotFoundException("Administrador no encontrado.");
            }
            if (admin.UserType != UserType.Administrador)
            {
                Log.Warning($"El usuario con ID {adminId} no tiene permisos de administrador.");
                throw new UnauthorizedAccessException("El usuario no tiene permisos de administrador.");
            }
            // Validar y ajustar parámetros de paginación
            var (allUsers, totalCount) = await _userRepository.GetFilteredForAdminAsync(adminId, searchParams);
            if (allUsers == null)
            {
                Log.Error("Error al obtener la lista de usuarios para el administrador.");
                throw new ArgumentNullException("Error al obtener la lista de usuarios.");
            }
            var pageSize = searchParams.PageSize ?? _defaultPageSize;
            var totalPages = (int)
                Math.Ceiling((double)totalCount / pageSize);
            var currentPage = searchParams.PageNumber;
            if (currentPage < 1 || currentPage > totalPages)
            {
                Log.Warning($"Página solicitada {currentPage} fuera de rango. Total de páginas: {totalPages}. Se ajusta a la página 1.");
                currentPage = 1;
            }
            // Aplicar paginación
            Log.Information($"Administrador con ID {adminId} obtuvo {totalCount} usuarios (página {currentPage} de {totalPages}).");
            return new UsersForAdminDTO
            {
                Users = allUsers.Adapt<List<UserForAdminDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<UserProfileForAdminDTO> GetUserProfileByIdAsync(int adminId, int userId)
        {
            //TODO Revisar si hay que hacer algo especial con el administrador.

            var user = await _userRepository.GetByIdWithRelationsAsync(userId);
            if (user == null) throw new KeyNotFoundException();

            return user.Adapt<UserProfileForAdminDTO>();
        }
    }
}