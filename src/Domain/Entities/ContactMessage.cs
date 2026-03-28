namespace Domain.Entities;

/// <summary>
/// Inbound message from the public contact form (visitor → you).
/// Admin inbox address and SMTP/API keys stay in configuration, not in this table.
/// </summary>
public class ContactMessage
{
    public Guid Id { get; set; }

    public string VisitorName { get; set; } = string.Empty;

    public string VisitorEmail { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string MessageBody { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Optional; useful for abuse/rate limiting. Omit if you want minimal PII.</summary>
    public string? SubmitterIpAddress { get; set; }

    public EmailNotificationStatus AdminNotificationStatus { get; set; }

    public DateTime? AdminNotificationCompletedAtUtc { get; set; }

    public string? AdminNotificationError { get; set; }
}

public enum EmailNotificationStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
}
