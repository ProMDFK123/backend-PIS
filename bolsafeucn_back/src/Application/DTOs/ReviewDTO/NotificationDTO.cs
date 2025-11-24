public class NotificationDTO
{
    public int Id { get; set; }
    public string UserEmail { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; } = false;
}
