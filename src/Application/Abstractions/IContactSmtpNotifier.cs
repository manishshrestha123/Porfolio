using Domain.Entities;

namespace Application.Abstractions;

public sealed record ContactSmtpSendResult(bool Sent, string? ErrorMessage);

public interface IContactSmtpNotifier
{
    /// <summary>
    /// Sends a plain-text email to the admin inbox (SMTP). On failure, <see cref="ContactSmtpSendResult.ErrorMessage"/> is set.
    /// </summary>
    Task<ContactSmtpSendResult> SendAsync(ContactMessage message, CancellationToken cancellationToken = default);
}
