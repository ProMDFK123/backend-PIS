public class AdminNotification
{
    public int Id { get; set; }
    required public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
