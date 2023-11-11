using System.Net;
using System.Net.Mail;
using System.Net.Mime;

using Adeotek.DevOpsTools.Helpers.Models;

namespace Adeotek.DevOpsTools.Helpers;

public static class EmailHelper
{
    public static void SendEmail(EmailConfig config)
    {
        if (!IsValidEmailAddress(config.FromAddress))
        {
            throw new ArgumentException("Invalid From email address!");
        }
        
        if (!IsValidEmailAddress(config.ToAddress))
        {
            throw new ArgumentException("Invalid To email address!");
        }

        string messageBody;
        if (config.ReadBodyFromFile)
        {
            if (!File.Exists(config.MessageBody))
            {
                throw new ArgumentException("Invalid Message Body source file!");
            }

            messageBody = File.ReadAllText(config.MessageBody);
        }
        else
        {
            messageBody = config.MessageBody;
        }
        
        var message = new MailMessage
        {
            From = new MailAddress(config.FromAddress, config.FromLabel)
        };
        message.To.Add(new MailAddress(config.ToAddress));
        message.Subject = config.Subject ?? string.Empty;
        message.Priority = MailPriority.High;
        message.IsBodyHtml = true;
        message.Body = messageBody;
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(messageBody, null, MediaTypeNames.Text.Html));
        
        using SmtpClient smtpClient = GetSmtpClient(config.SmtpConfig);
        smtpClient.Send(message);
    }
        
    private static SmtpClient GetSmtpClient(SmtpConfig config)
    {
        if (string.IsNullOrEmpty(config.Host) || config.Port <= 0)
        {
            throw new ArgumentException("Invalid SMTP configuration");
        }
        
        var client = new SmtpClient
        {
            Host = config.Host,
            Port = config.Port,
            EnableSsl = config.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = config.Timeout
        };
        if (!string.IsNullOrEmpty(config.User))
        {
            client.Credentials = new NetworkCredential(config.User, config.Password);
        }
        return client;
    }
    
    private static bool IsValidEmailAddress(string email)
    { 
        try
        { 
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}