using System.Net;
using System.Net.Sockets;

using Adeotek.DevOpsTools.CommandsSettings.Networking;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Networking;

internal class PortListenCommand : CommandBase<PortListenSettings>
{
    protected override string CommandName => "port listen";
    
    protected override int ExecuteCommand(CommandContext context, PortListenSettings settings)
    {
        try
        {
            StartListener(settings);
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
        try
        {
            TcpListener listener  = new(ipAddress, settings.Port);
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                listener.Stop();
            };
            
            listener.Start();
            PrintListenerStarted(ipAddress, settings.Port);
            while (true)
            {
                var client = listener.AcceptTcpClient();
                PrintMessage($"{DateTime.Now:s} - Client connected!", _warningColor);
                Changes++;
                client.Close();
            }
        }
        catch (SocketException e)
        {
            if (e.ErrorCode != 10004)
            {
                throw;
            }
        }
        finally
        {
            PrintMessage("Listener stopped.", _standardColor);
        }
    }
    
    protected virtual void PrintListenerStarted(IPAddress ipAddress, int port)
    {
        if (IsSilent)
        {
            return;
        }
        
        var composer = new CustomComposer()
            .Style(_standardColor, "Listening on:").Space()
            .Style(_successColor, $"{ipAddress}:{port}").LineBreak()
            .Style(_standardColor, "Press").Space()
            .Style(_verboseColor, "CTRL+C").Space()
            .Style(_standardColor, "to exit...").LineBreak();
        AnsiConsole.Write(composer);
    }
}