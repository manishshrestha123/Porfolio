using Application.Abstractions;
using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email;

public sealed class SmtpContactNotifier : IContactSmtpNotifier
{
    private readonly SmtpOptions _opt;
    private readonly ILogger<SmtpContactNotifier> _logger;

    public SmtpContactNotifier(IOptions<SmtpOptions> options, ILogger<SmtpContactNotifier> logger)
    {
        _opt = options.Value;
        _logger = logger;
    }

    public async Task<ContactSmtpSendResult> SendAsync(
        ContactMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_opt.Host))
                throw new InvalidOperationException("Smtp:Host is not configured.");
            if (string.IsNullOrWhiteSpace(_opt.AdminEmail))
                throw new InvalidOperationException("Smtp:AdminEmail is not configured.");
            if (string.IsNullOrWhiteSpace(_opt.FromEmail))
                throw new InvalidOperationException("Smtp:FromEmail is not configured.");

            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
            mime.To.Add(MailboxAddress.Parse(_opt.AdminEmail));
            mime.ReplyTo.Add(new MailboxAddress(message.VisitorName, message.VisitorEmail));

            var subjectLine = string.IsNullOrWhiteSpace(message.Subject)
                ? "Contact form"
                : message.Subject;
            mime.Subject = $"[Portfolio] {subjectLine}";

            mime.Body = new TextPart("plain") { Text = BuildPlainTextBody(message) };

            using var client = new SmtpClient();
            var secure = ResolveSecureSocketOption(_opt);
            await client.ConnectAsync(_opt.Host, _opt.Port, secure, cancellationToken);

            if (!string.IsNullOrEmpty(_opt.UserName))
            {
                await client.AuthenticateAsync(_opt.UserName, _opt.Password ?? string.Empty, cancellationToken);
            }

            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return new ContactSmtpSendResult(Sent: true, ErrorMessage: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP send failed for contact message {MessageId}", message.Id);
            var msg = ex.Message;
            if (msg.Length > 2000)
                msg = msg[..2000];
            return new ContactSmtpSendResult(Sent: false, ErrorMessage: msg);
        }
    }

    private static string BuildPlainTextBody(ContactMessage m)
    {
        return
            $"""
            You have a new portfolio contact message.

            Name: {m.VisitorName}
            Email: {m.VisitorEmail}
            Subject: {m.Subject ?? "(none)"}
            Submitted (UTC): {m.CreatedAtUtc:O}

            Message:
            {m.MessageBody}

            ---
            Message ID: {m.Id}
            IP: {m.SubmitterIpAddress ?? "unknown"}
            """;
    }

    private static SecureSocketOptions ResolveSecureSocketOption(SmtpOptions opt)
    {
        return opt.Security switch
        {
            SmtpSecurityMode.None => SecureSocketOptions.None,
            SmtpSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            SmtpSecurityMode.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurityMode.Auto => opt.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.StartTls,
        };
    }
}
