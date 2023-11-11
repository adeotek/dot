namespace Adeotek.DevOpsTools.Helpers.Models;

public class EmailConfig
{
    public SmtpConfig SmtpConfig { get; set; } = default!;
    public string FromAddress { get; set; } = default!;
    public string? FromLabel { get; set; }
    public string ToAddress { get; set; } = default!;
    public string? MessageSubject { get; set; }
    public string MessageBody { get; set; } = default!;
    public bool ReadBodyFromFile { get; set; }
}