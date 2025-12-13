using bolsafeucn_back.src.models;

namespace bolsafeucn_back.src.interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario> AddAsync(Usuario usuario);
        Task<bool> DeleteAsync(int id);
    }
}
