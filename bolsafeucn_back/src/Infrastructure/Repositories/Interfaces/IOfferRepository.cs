using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;

public interface IOfferRepository
{
    // Métodos de consulta
    Task<Offer?> GetByIdAsync(int offerId);
    Task<Offer?> GetOfferByIdAsync(int id);
    Task<IEnumerable<Offer>> GetAllActiveAsync();
    Task<IEnumerable<Offer>> GetAllOffersAsync();
    Task<IEnumerable<Offer>> GetOffersByUserIdAsync(int userId);
    Task<IEnumerable<Offer>> GetAllPendingOffersAsync();
    Task<IEnumerable<Offer>> PublishedOffersAsync();
    Task<Offer> CreateOfferAsync(Offer offer);
    Task<bool> UpdateOfferAsync(Offer offer);
    Task<bool> DeleteOfferAsync(int id);
}
