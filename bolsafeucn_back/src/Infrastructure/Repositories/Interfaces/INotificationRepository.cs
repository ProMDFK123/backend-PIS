public interface INotificationRepository
{
    Task AddAsync(NotificationDTO notification);
    Task<List<NotificationDTO>> GetByUserEmailAsync(string email);
}
