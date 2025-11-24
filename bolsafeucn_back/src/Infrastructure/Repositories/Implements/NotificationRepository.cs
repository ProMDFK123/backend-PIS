using bolsafeucn_back.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationDTO notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationDTO>> GetByUserEmailAsync(string email)
    {
        return await _context.Notifications
            .Where(n => n.UserEmail == email)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}