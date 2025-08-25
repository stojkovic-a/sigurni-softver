namespace Auth.app.Data.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccuredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool? Succeeded { get; set; } = null;
}
