namespace Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public SmtpSecurityMode Security { get; set; } = SmtpSecurityMode.Auto;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "Portfolio";

    /// <summary>Where contact notifications are delivered (your inbox).</summary>
    public string AdminEmail { get; set; } = string.Empty;
}

public enum SmtpSecurityMode
{
    Auto,
    None,
    StartTls,
    SslOnConnect,
}
