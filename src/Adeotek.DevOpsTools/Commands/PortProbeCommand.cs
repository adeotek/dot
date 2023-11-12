using System.Net;
using System.Net.Sockets;

using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class PortProbeCommand : CommandBase<PortProbeSettings>
{
    protected override string CommandName => "port probe";
    
    protected override int ExecuteCommand(CommandContext context, PortProbeSettings settings)
    {
        try
        {
            ProbePort(settings);
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

    private void ProbePort(PortProbeSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Host))
        {
            throw new ArgumentException("Invalid value provided", nameof(settings.Host));
        }
        
        if (settings.Port is <= 0 or > 65535)
        {
            throw new ArgumentException("Invalid value provided", nameof(settings.Port));
        }

        IPAddress ipAddress;
        if (settings.Host.ToLower() == "localhost")
        {
            ipAddress = IPAddress.Parse("127.0.0.1");
        }
        else
        {
            var ipAddresses = Dns.GetHostAddresses(settings.Host);
            if (ipAddresses.Length == 0)
            {
                throw new ArgumentException($"Unable to get the IP address of host: {settings.Host}");
            }
            ipAddress = ipAddresses[0];
        }
        
        try
        {
            Socket socket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAddress, settings.Port);
            PrintResult(settings.Host, settings.Port, ipAddress);
            Changes++;
        }
        catch (SocketException e)
        {
            PrintResult(settings.Host, settings.Port, ipAddress, e.Message);
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
        }
    }
    
    protected virtual void PrintResult(string host, int port, IPAddress ipAddress, string? error = null)
    {
        if (IsSilent)
        {
            return;
        }
        
        var composer = new CustomComposer()
            .Style(_standardColor, "Connection to").Space()
            .Style(_verboseColor, $"{host} ({ipAddress})").Space()
            .Style(_standardColor, "on").Space()
            .Style(_verboseColor, $"{port} [tcp]").Space();
        
        if (string.IsNullOrEmpty(error))
        {
            composer.Style(_successColor, "succeeded").Style(_standardColor, "!").LineBreak();
        }
        else
        {
            composer.Style(_errorColor, "failed:").LineBreak()
                .Style(_errorColor, error).LineBreak();
        }
        
        AnsiConsole.Write(composer);
    }
}