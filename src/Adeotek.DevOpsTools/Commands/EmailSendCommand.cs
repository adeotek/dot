using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.Helpers;
using Adeotek.DevOpsTools.Helpers.Models;
using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class EmailSendCommand : CommandBase<EmailSettings>
{
    private const int NameLength = 18;
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    protected override int ExecuteCommand(CommandContext context, EmailSettings settings)
    {
        try
        {
            var config = GetConfiguration(settings);
            PrintEmailConfig(config);
            EmailHelper.SendEmail(config);
            Changes++;
            PrintMessage("Email successfully sent!", _successColor, separator: true);
            return 0;
        }
        catch (ShellCommandException e)
        {
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return e.ExitCode;
        }
        catch (Exception e)
        {
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return 1;
        }
    }

    private static EmailConfig GetConfiguration(EmailSettings settings) =>
        !string.IsNullOrEmpty(settings.ConfigFile)
            ? LoadConfiguration(settings)
            : new EmailConfig
            {
                FromAddress = settings.FromAddress ?? throw new ArgumentException("Invalid option", nameof(settings.FromAddress)),
                ToAddress = settings.ToAddress ?? throw new ArgumentException("Invalid option", nameof(settings.ToAddress)),
                MessageSubject = settings.MessageSubject ?? throw new ArgumentException("Invalid option", nameof(settings.MessageSubject)),
                MessageBody = settings.MessageBody ?? throw new ArgumentException("Invalid option", nameof(settings.MessageBody)),
                ReadBodyFromFile = settings.ReadBodyFromFile ?? false,
                SmtpConfig = new SmtpConfig
                {
                    Host = settings.SmtpHost ?? throw new ArgumentException("Invalid option", nameof(settings.SmtpHost)),
                    Port = settings.SmtpPort ?? 25,
                    User = settings.SmtpUser,
                    Password = settings.SmtpPassword,
                    UseSsl = settings.UseSsl ?? false
                }
            };

    private static EmailConfig LoadConfiguration(EmailSettings settings)
    {
        var config = ConfigManager.LoadConfig<EmailConfig>(settings.ConfigFile);
        // Override config file values with command options
        if (!string.IsNullOrEmpty(settings.FromAddress))
        {
            config.FromAddress = settings.FromAddress;
        }
        if (!string.IsNullOrEmpty(settings.ToAddress))
        {
            config.ToAddress = settings.ToAddress;
        }
        if (settings.MessageSubject is not null)
        {
            config.MessageSubject = settings.MessageSubject;
        }
        if (!string.IsNullOrEmpty(settings.MessageBody))
        {
            config.MessageBody = settings.MessageBody;
        }
        if (settings.ReadBodyFromFile is not null)
        {
            config.ReadBodyFromFile = settings.ReadBodyFromFile.Value;
        }
        // Override smtp config file values with command options
        if (!string.IsNullOrEmpty(settings.SmtpHost))
        {
            config.SmtpConfig.Host = settings.SmtpHost;
        }

        if (settings.SmtpPort is not null)
        {
            config.SmtpConfig.Port = settings.SmtpPort.Value;
        }
        if (settings.SmtpUser is not null)
        {
            config.SmtpConfig.User = settings.SmtpUser;
        }
        if (settings.SmtpPassword is not null)
        {
            config.SmtpConfig.Password = settings.SmtpPassword;
        }
        if (settings.UseSsl is not null)
        {
            config.SmtpConfig.UseSsl = settings.UseSsl.Value;
        }
        return config;
    }

    private void PrintEmailConfig(EmailConfig config)
    {
        if (IsSilent)
        {
            return;
        }

        if (IsVerbose)
        {
            PrintSeparator();    
        }
        
        var composer = new CustomComposer()
            .Style(SpecialColor, "Sending email message").LineBreak();
        if (string.IsNullOrEmpty(config.FromLabel))
        {
            composer.Style(LabelColor, "From:", NameLength).Style(ValueColor, config.FromAddress)
                .LineBreak();
        }
        else
        {
            composer.Style(LabelColor, "From:", NameLength).Style(ValueColor, $"{config.FromLabel}<{config.FromAddress}>")
                .LineBreak();
        }
        composer
            .Style(LabelColor, "To:", NameLength).Style(ValueColor, config.ToAddress).LineBreak()
            .Style(LabelColor, "With subject:", NameLength).Style(ValueColor, config.MessageSubject ?? "").LineBreak()
            .Style(LabelColor, "Body source:", NameLength).Style(SpecialValueColor, config.ReadBodyFromFile ? "File" : "Inline text").LineBreak()
            .Style(SpecialColor, "Via SMTP").LineBreak()
            .Style(LabelColor, "Host:", NameLength).Style(ValueColor, config.SmtpConfig.Host).LineBreak()
            .Style(LabelColor, "Port:", NameLength).Style(ValueColor, config.SmtpConfig.Port.ToString()).LineBreak()
            .Style(LabelColor, "Using SSL/TLS:", NameLength).Style(ValueColor, config.SmtpConfig.UseSsl.ToString()).LineBreak();
        if (!string.IsNullOrEmpty(config.SmtpConfig.User))
        {
            composer
                .Style(LabelColor, "User:", NameLength).Style(ValueColor, config.SmtpConfig.User).LineBreak()
                .Style(LabelColor, "Password:", NameLength).Style(SpecialValueColor, "********").LineBreak();
        }
        composer.Style(LabelColor, "Timeout:", NameLength).Style(ValueColor, $"{config.SmtpConfig.Timeout / 1000.000} sec.").LineBreak();

        AnsiConsole.Write(composer);
    }
}