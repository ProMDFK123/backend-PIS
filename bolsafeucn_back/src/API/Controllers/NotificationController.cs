using bolsafeucn_back.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("test-status-change")]
    public async Task<IActionResult> TestStatusChange([FromBody] PostulationStatusChangedEvent evt)
    {
        await _notificationService.SendPostulationStatusChangeAsync(evt);
        return Ok(new { message = "Notificación procesada correctamente." });
    }
}
