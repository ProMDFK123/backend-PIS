public interface INotificationService
{
    Task SendPostulationStatusChangeAsync(PostulationStatusChangedEvent evt);
}
