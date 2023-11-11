using System.Net;
using System.Net.Sockets;

using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class PortListenCommand : CommandBase<PortListenSettings>
{
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    protected override string CommandName => "port";
    
    protected override int ExecuteCommand(CommandContext context, PortListenSettings settings)
    {
        try
        {
            StartListener(settings);
            // PrintMessage("Email successfully sent!", _successColor, separator: true);
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

    private void StartListener(PortListenSettings settings)
    {
        if (settings.Port is <= 0 or > 65535)
        {
            throw new ArgumentException("Invalid value provided", nameof(settings.Port));
        }

        var ipAddress = string.IsNullOrEmpty(settings.IpAddress)
            ? IPAddress.Any
            : IPAddress.Parse(settings.IpAddress);
        TcpListener listener  = new(ipAddress, settings.Port);
        try
        {
            var stopListening = false;
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                stopListening = true;
            };
            listener.Start();
            while (!stopListening)
            {
                PrintMessage($"Listening on port: {settings.Port}", _standardColor);
                PrintMessage("Press CTRL+C to exit...", _standardColor);
                var client = listener.AcceptTcpClient();
                PrintMessage("Client connected!", _successColor);
                client.Close();
            }
        }
        finally
        {
            listener.Stop();
            PrintMessage("Listener stopped.", _standardColor);
        }
    }
}