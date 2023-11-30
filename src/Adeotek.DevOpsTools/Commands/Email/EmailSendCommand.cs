using Adeotek.DevOpsTools.CommandsSettings.Email;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.Helpers;
using Adeotek.DevOpsTools.Helpers.Models;
using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Email;

internal class EmailSendCommand : CommandBase<EmailSendSettings>
{
    private const int NameLength = 18;
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    protected override string CommandName => "email";
    
    protected override int ExecuteCommand(CommandContext context, EmailSendSettings settings)
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

    private static EmailConfig GetConfiguration(EmailSendSettings sendSettings) =>
        !string.IsNullOrEmpty(sendSettings.ConfigFile)
            ? LoadConfiguration(sendSettings)
            : new EmailConfig
            {
                FromAddress = sendSettings.FromAddress ?? throw new ArgumentException("Invalid option", nameof(sendSettings.FromAddress)),
                ToAddress = sendSettings.ToAddress ?? throw new ArgumentException("Invalid option", nameof(sendSettings.ToAddress)),
                MessageSubject = sendSettings.MessageSubject ?? throw new ArgumentException("Invalid option", nameof(sendSettings.MessageSubject)),
                MessageBody = sendSettings.MessageBody ?? throw new ArgumentException("Invalid option", nameof(sendSettings.MessageBody)),
                ReadBodyFromFile = sendSettings.ReadBodyFromFile ?? false,
                SmtpConfig = new SmtpConfig
                {
                    Host = sendSettings.SmtpHost ?? throw new ArgumentException("Invalid option", nameof(sendSettings.SmtpHost)),
                    Port = sendSettings.SmtpPort ?? 25,
                    User = sendSettings.SmtpUser,
                    Password = sendSettings.SmtpPassword,
                    UseSsl = sendSettings.UseSsl ?? false
                }
            };

    private static EmailConfig LoadConfiguration(EmailSendSettings sendSettings)
    {
        var config = new ConfigManager()
            .LoadConfig<EmailConfig>(sendSettings.ConfigFile);
        // Override config file values with command options
        if (!string.IsNullOrEmpty(sendSettings.FromAddress))
        {
            config.FromAddress = sendSettings.FromAddress;
        }
        if (!string.IsNullOrEmpty(sendSettings.ToAddress))
        {
            config.ToAddress = sendSettings.ToAddress;
        }
        if (sendSettings.MessageSubject is not null)
        {
            config.MessageSubject = sendSettings.MessageSubject;
        }
        if (!string.IsNullOrEmpty(sendSettings.MessageBody))
        {
            config.MessageBody = sendSettings.MessageBody;
        }
        if (sendSettings.ReadBodyFromFile is not null)
        {
            config.ReadBodyFromFile = sendSettings.ReadBodyFromFile.Value;
        }
        // Override smtp config file values with command options
        if (!string.IsNullOrEmpty(sendSettings.SmtpHost))
        {
            config.SmtpConfig.Host = sendSettings.SmtpHost;
        }

        if (sendSettings.SmtpPort is not null)
        {
            config.SmtpConfig.Port = sendSettings.SmtpPort.Value;
        }
        if (sendSettings.SmtpUser is not null)
        {
            config.SmtpConfig.User = sendSettings.SmtpUser;
        }
        if (sendSettings.SmtpPassword is not null)
        {
            config.SmtpConfig.Password = sendSettings.SmtpPassword;
        }
        if (sendSettings.UseSsl is not null)
        {
            config.SmtpConfig.UseSsl = sendSettings.UseSsl.Value;
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