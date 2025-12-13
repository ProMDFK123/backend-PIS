public interface IAdminNotificationRepository
{
    Task AddAsync(AdminNotification notification);
    Task<IEnumerable<AdminNotification>> GetAllAsync();
    Task<AdminNotification?> GetByIdAsync(int id);
    Task UpdateAsync(AdminNotification notification);

}
