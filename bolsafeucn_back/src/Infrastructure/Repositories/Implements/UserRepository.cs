using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<GeneralUser> _userManager;

        public UserRepository(AppDbContext context, UserManager<GeneralUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <returns>El usuario encontrado o null si no existe.</returns>
        public async Task<GeneralUser?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// Verifica si un correo electrónico ya está registrado.
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <returns>True si el correo electrónico ya está registrado, de lo contrario false.</returns>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Verifica si un RUT ya está registrado.
        /// </summary>
        /// <param name="rut">RUT del estudiante</param>
        /// <returns>True si el RUT ya está registrado, de lo contrario false.</returns>
        public async Task<bool> ExistsByRutAsync(string rut)
        {
            return await _context.Users.AnyAsync(e => e.Rut == rut);
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="user">Usuario a crear</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>True si se creó el usuario, de lo contrario false.</returns>
        public async Task<bool> CreateUserAsync(GeneralUser user, string password, string role)
        {
            Log.Information(
                "Creando usuario en la base de datos: {Email}, Rol: {Role}",
                user.Email,
                role
            );
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                Log.Information(
                    "Usuario creado exitosamente: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                var userRole = await _userManager.AddToRoleAsync(user, role);
                if (userRole.Succeeded)
                {
                    Log.Information(
                        "Rol {Role} asignado exitosamente a usuario ID: {UserId}",
                        role,
                        user.Id
                    );
                }
                else
                {
                    Log.Error(
                        "Error al asignar rol {Role} a usuario ID: {UserId}. Errores: {Errors}",
                        role,
                        user.Id,
                        string.Join(", ", userRole.Errors.Select(e => e.Description))
                    );
                }
                return userRole.Succeeded;
            }
            Log.Error(
                "Error al crear usuario {Email}. Errores: {Errors}",
                user.Email,
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
            return false;
        }

        /// <summary>
        /// Crea un nuevo estudiante en el sistema.
        /// </summary>
        /// <param name="student">Estudiante a crear</param>
        /// <returns>True si se creó el estudiante, de lo contrario false.</returns>
        public async Task<bool> CreateStudentAsync(Student student)
        {
            Log.Information(
                "Creando perfil de estudiante para usuario ID: {UserId}",
                student.GeneralUserId
            );
            var result = await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            Log.Information(
                "Perfil de estudiante creado exitosamente para usuario ID: {UserId}",
                student.GeneralUserId
            );
            return result != null;
        }

        public async Task<bool> CreateIndividualAsync(Individual individual)
        {
            Log.Information(
                "Creando perfil de particular para usuario ID: {UserId}",
                individual.GeneralUserId
            );
            var result = await _context.Individuals.AddAsync(individual);
            await _context.SaveChangesAsync();
            Log.Information(
                "Perfil de particular creado exitosamente para usuario ID: {UserId}",
                individual.GeneralUserId
            );
            return result != null;
        }

        /// <summary>
        /// Crea una nueva empresa en el sistema.
        /// </summary>
        /// <param name="company">Empresa a crear</param>
        /// <returns>True si se creó la empresa, de lo contrario false.</returns>
        public async Task<bool> CreateCompanyAsync(Company company)
        {
            Log.Information(
                "Creando perfil de empresa para usuario ID: {UserId}",
                company.GeneralUserId
            );
            var result = await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
            Log.Information(
                "Perfil de empresa creado exitosamente para usuario ID: {UserId}",
                company.GeneralUserId
            );
            return result != null;
        }

        /// <summary>
        /// Crea un nuevo administrador en el sistema.
        /// </summary>
        /// <param name="admin">Administrador a crear</param>
        /// <param name="superAdmin">Indica si el administrador es superadministrador</param>
        /// <returns>True si se creó el administrador, de lo contrario false.</returns>
        public async Task<bool> CreateAdminAsync(Admin admin, bool superAdmin)
        {
            Log.Information(
                "Creando perfil de admin para usuario ID: {UserId}, SuperAdmin: {SuperAdmin}",
                admin.GeneralUserId,
                superAdmin
            );
            var result = await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            Log.Information(
                "Perfil de admin creado exitosamente para usuario ID: {UserId}",
                admin.GeneralUserId
            );
            return result != null;
        }

        /// <summary>
        /// Confirma el correo electrónico de un usuario.
        /// </summary>
        /// <param name="email">Correo electrónico a confirmar</param>
        /// <returns>True si se confirmó el correo electrónico, de lo contrario false.</returns>
        public async Task<bool> ConfirmEmailAsync(string email)
        {
            Log.Information("Confirmando email en base de datos: {Email}", email);
            var result = await _context
                .Users.Where(u => u.Email == email)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.EmailConfirmed, true));

            if (result > 0)
            {
                Log.Information("Email confirmado exitosamente en base de datos: {Email}", email);
            }
            else
            {
                Log.Warning("No se pudo confirmar email en base de datos: {Email}", email);
            }
            return result > 0;
        }

        /// <summary>
        /// Verifica si la contraseña proporcionada coincide con la del usuario.
        /// </summary>
        /// <param name="user">Usuario a verificar</param>
        /// <param name="password">Contraseña a verificar</param>
        /// <returns>True si la contraseña es correcta, de lo contrario false.</returns>
        public async Task<bool> CheckPasswordAsync(GeneralUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// Actualiza la información de un usuario.
        /// </summary>
        /// <param name="user">Usuario a actualizar</param>
        /// <returns>True si la actualización fue exitosa, de lo contrario false.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> UpdateAsync(GeneralUser user)
        {
            Log.Information($"Actualizando informacion para el usuario Id: {user.Id}");
            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(" | ", userResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                Log.Error("Error al actualizar usuario Id: {UserId}. Identity errors: {Errors}", user.Id, errors);
                throw new InvalidOperationException($"Error al actualizar los datos del usuario: {errors}");
            }
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario.
        /// </summary>
        /// <param name="user">Usuario al que se le actualizará la contraseña</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>True si la contraseña se actualizó correctamente, de lo contrario false.</returns>
        public async Task<bool> UpdatePasswordAsync(GeneralUser user, string newPassword)
        {
            Log.Information($"Actualizando contraseña para usuario ID: {user.Id}");
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                Log.Error(
                    "Error al eliminar la contraseña actual para usuario ID: {UserId}. Errores: {Errors}",
                    user.Id,
                    string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))
                );
                return false;
            }

            var newPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!newPasswordResult.Succeeded)
            {
                Log.Error(
                    "Error al actualizar contraseña para usuario ID: {UserId}. Errores: {Errors}",
                    user.Id,
                    string.Join(", ", newPasswordResult.Errors.Select(e => e.Description))
                );
                return false;
            }

            Log.Information(
                "Contraseña actualizada exitosamente para usuario ID: {UserId}",
                user.Id
            );
            return newPasswordResult.Succeeded;
        }

        /// <summary>
        /// Obtiene el rol del usuario.
        /// </summary>
        /// <param name="user">Usuario del cual obtener el rol</param>
        /// <returns>Rol del usuario</returns>
        public async Task<string> GetRoleAsync(GeneralUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault()!;
        }

        /// <summary>
        /// Obtiene un usuario general por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a obtener</param>
        /// <returns>Usuario general</returns>
        public async Task<GeneralUser> GetGeneralUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return user!;
        }

        public async Task<IEnumerable<GeneralUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<GeneralUser?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<GeneralUser?> GetByIdWithRelationsAsync(int userId)
        {
            return await _context
                .Users.Include(u => u.Student)
                .Include(u => u.Company)
                .Include(u => u.Individual)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Obtiene un usuario por su ID y tipo de usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Usuario general con las relaciones correspondientes</returns>
        public async Task<GeneralUser?> GetUntrackedWithTypeAsync(int userId, UserType userType)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();
            switch (userType)
            {
                case UserType.Estudiante: query = query.Include(u => u.Student); break;
                case UserType.Particular: query = query.Include(u => u.Individual); break;
                case UserType.Empresa: query = query.Include(u => u.Company); break;
                case UserType.Administrador: query = query.Include(u => u.Admin); break;
                default: return null;
            };
            return await query
                            .Include(u => u.ProfilePhoto)
                            .Include(u => u.ProfileBanner)
                            .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Obtiene un usuario por su ID y tipo de usuario para actualización.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Usuario general con las relaciones correspondientes</returns>
        public async Task<GeneralUser?> GetTrackedWithTypeAsync(int userId, UserType userType)
        {
            var query = _context.Users.AsQueryable();
            switch (userType)
            {
                case UserType.Estudiante: query = query.Include(u => u.Student); break;
                case UserType.Particular: query = query.Include(u => u.Individual); break;
                case UserType.Empresa: query = query.Include(u => u.Company); break;
                case UserType.Administrador: query = query.Include(u => u.Admin); break;
                default: return null;
            };
            return await query
                            .Include(u => u.ProfilePhoto)
                            .Include(u => u.ProfileBanner)
                            .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<GeneralUser> AddAsync(GeneralUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>True si se eliminó el usuario, de lo contrario false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            Log.Information("Intentando eliminar usuario ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                Log.Warning("Usuario ID: {UserId} no encontrado para eliminación", id);
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            Log.Information("Usuario ID: {UserId} eliminado exitosamente", id);
            return true;
        }
    }
}
