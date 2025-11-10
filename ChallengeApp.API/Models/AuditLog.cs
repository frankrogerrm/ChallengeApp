public class AuditLog
{
    public int Id { get; set; }
    public string UserEmail { get; set; }
    public string Action { get; set; }
    public string Entity { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}
