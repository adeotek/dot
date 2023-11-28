namespace Adeotek.DevOpsTools.Helpers.Models;

public class SmtpConfig
{
    public string Host { get; set; } = default!;
    public int Port { get; set; } = 25;
    public string? User { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
    public int Timeout { get; set; } = 60 * 1000;
}