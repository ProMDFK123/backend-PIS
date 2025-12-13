using bolsafeucn_back.src.dtos;
using bolsafeucn_back.src.models;

namespace bolsafeucn_back.src.interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioAsync(int id);
        Task<Usuario> CrearUsuarioAsync(UsuarioDto dto);
        Task<bool> EliminarUsuarioAsync(int id);
    }
}
