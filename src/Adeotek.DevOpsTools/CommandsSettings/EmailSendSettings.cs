using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

public class EmailSendSettings : GlobalSettings
{
    [Description("Config file (with absolute or relative path)")]
    [CommandOption("-c|--config <value>")]
    public string? ConfigFile { get; init; }
    
    [Description("SMTP Host")]
    [CommandOption("--host <value>")]
    public string? SmtpHost { get; init; }

    [Description("SMTP Port (default 25)")]
    [CommandOption("--port <value>")]
    public int? SmtpPort { get; init; }
    
    [Description("Use SSL/TSL (default false)")]
    [CommandOption("--use-ssl")]
    public bool? UseSsl { get; init; }
    
    [Description("SMTP User")]
    [CommandOption("--user <value>")]
    public string? SmtpUser { get; init; }
    
    [Description("SMTP Password")]
    [CommandOption("--password <value>")]
    public string? SmtpPassword { get; init; }
    
    [Description("From Address")]
    [CommandOption("--from <value>")]
    public string? FromAddress { get; init; }
    
    [Description("To Address")]
    [CommandOption("--to <value>")]
    public string? ToAddress { get; init; }
    
    [Description("Email message Body as string or file (accepted formats are plain text and HTML)")]
    [CommandOption("--body <value>")]
    public string? MessageBody { get; init; }
    
    [Description("Email message Subject")]
    [CommandOption("--subject <value>")]
    public string? MessageSubject { get; init; }
    
    [Description("Read Email message Body from file (default false). The file must be supplied to `-b|--body` option.")]
    [CommandOption("--body-from-file")]
    public bool? ReadBodyFromFile { get; init; }
}