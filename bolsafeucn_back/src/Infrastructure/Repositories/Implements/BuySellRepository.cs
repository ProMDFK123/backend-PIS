using Bogus.Bson;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Repositorio para gestionar publicaciones de compra/venta
    /// </summary>
    public class BuySellRepository : IBuySellRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BuySellRepository> _logger;

        public BuySellRepository(AppDbContext context, ILogger<BuySellRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BuySell> CreateBuySellAsync(BuySell buySell)
        {
            try
            {
                _context.BuySells.Add(buySell);
                await _context.SaveChangesAsync();
                return buySell;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la publicación de compra/venta", ex);
            }
        }

        public async Task<IEnumerable<BuySell>> GetAllActiveAsync()
        {
            return await _context
                .BuySells.Include(bs => bs.User)
                .ThenInclude(u => u.Company)
                .Include(bs => bs.User)
                .ThenInclude(u => u.Individual)
                .Include(bs => bs.Images)
                .Where(bs => bs.IsActive)
                .OrderByDescending(bs => bs.PublicationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<BuySell>> GetAllPendingBuySellsAsync()
        {
            _logger.LogInformation(
                "Consultando publicaciones de compra/venta pendientes en la base de datos"
            );
            var buysell = await _context
                .BuySells.Include(bs => bs.User)
                .ThenInclude(u => u.Company)
                .Include(bs => bs.User)
                .ThenInclude(u => u.Individual)
                .Include(bs => bs.Images)
                .Where(bs => bs.statusValidation == StatusValidation.InProcess)
                .OrderByDescending(bs => bs.PublicationDate)
                .AsNoTracking()
                .ToListAsync();
            _logger.LogInformation(
                "Consulta completada: {Count} publicaciones de compra/venta pendientes encontradas",
                buysell.Count
            );
            return buysell;
        }

        public async Task<IEnumerable<BuySell>> GetPublishedBuySellsAsync()
        {
            _logger.LogInformation(
                "Consultando publicaciones de compra/venta publicadas en la base de datos"
            );
            var buysell = await _context
                .BuySells.Include(bs => bs.User)
                .ThenInclude(u => u.Company)
                .Include(bs => bs.User)
                .ThenInclude(u => u.Individual)
                .Include(bs => bs.Images)
                .Where(bs => bs.statusValidation == StatusValidation.Published)
                .OrderByDescending(bs => bs.PublicationDate)
                .AsNoTracking()
                .ToListAsync();
            _logger.LogInformation(
                "Consulta completada: {Count} publicaciones de compra/venta publicadas encontradas",
                buysell.Count
            );
            return buysell;
        }

        public async Task<BuySell?> GetByIdAsync(int id)
        {
            return await _context
                .BuySells.Include(bs => bs.User)
                .Include(bs => bs.Images)
                .FirstOrDefaultAsync(bs => bs.Id == id);
        }

        public async Task<IEnumerable<BuySell>> GetByUserIdAsync(int userId)
        {
            return await _context
                .BuySells.Where(bs => bs.UserId == userId)
                .Include(bs => bs.Images)
                .OrderByDescending(bs => bs.PublicationDate)
                .ToListAsync();
        }

        public async Task<BuySell> UpdateAsync(BuySell buySell)
        {
            try
            {
                _context.BuySells.Update(buySell);
                await _context.SaveChangesAsync();
                return buySell;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar la publicación de compra/venta", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var buySell = await GetByIdAsync(id);
                if (buySell == null)
                    return false;

                buySell.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la publicación de compra/venta", ex);
            }
        }

        public async Task<IEnumerable<BuySell>> SearchByCategoryAsync(string category)
        {
            return await _context
                .BuySells.Where(bs =>
                    bs.IsActive && bs.Category.ToLower().Contains(category.ToLower())
                )
                .Include(bs => bs.User)
                .Include(bs => bs.Images)
                .OrderByDescending(bs => bs.PublicationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BuySell>> SearchByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice
        )
        {
            return await _context
                .BuySells.Where(bs => bs.IsActive && bs.Price >= minPrice && bs.Price <= maxPrice)
                .Include(bs => bs.User)
                .Include(bs => bs.Images)
                .OrderBy(bs => bs.Price)
                .ToListAsync();
        }
    }
}
