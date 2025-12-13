namespace bolsafeucn_back.src.Domain.Models
{
    public enum AuditAction
    {
        Blocked,
        Unblocked,
        Deleted,
        UpdatedProfile,
        Other
    }
    public class AdminLog : ModelBase
    {
        public GeneralUser? User { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}