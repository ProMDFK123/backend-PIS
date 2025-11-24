using bolsafeucn_back.src.Application.Services.Interfaces;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly INotificationRepository _notificationRepo;

    public NotificationService(IEmailService emailService, INotificationRepository notificationRepo)
    {
        _emailService = emailService;
        _notificationRepo = notificationRepo;
    }

    public async Task SendPostulationStatusChangeAsync(PostulationStatusChangedEvent evt)
    {
    
        var notification = new NotificationDTO
        {
            UserEmail = evt.StudentEmail,
            Message = $"Tu postulación a '{evt.OfferName}' en '{evt.CompanyName}' ha cambiado a '{evt.NewStatus}'.",
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepo.AddAsync(notification);
        await _emailService.SendPostulationStatusChangeEmailAsync(
            evt.StudentEmail,
            evt.OfferName,
            evt.CompanyName,
            evt.NewStatus
        );
    }
}
