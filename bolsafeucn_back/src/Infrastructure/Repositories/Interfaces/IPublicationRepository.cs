using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bolsafeucn_back.src.Domain.Models;
using Mapster;
using Resend;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Obteniene una publicacion cual sea, por la id de usuario y el estado de validación de la misma.
    /// </summary>
    public interface IPublicationRepository
    {
        
        Task<IEnumerable<Publication>> GetPublishedPublicationsByUserIdAsync(string userId);
        Task<IEnumerable<Publication>> GetRejectedPublicationsByUserIdAsync(string userId);
        Task<IEnumerable<Publication>> GetPendingPublicationsByUserIdAsync(string userId);
        
        Task<Publication?> GetByIdAsync(int id);

        Task UpdateAsync(Publication publication);
    }
}
