using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

public class PortListenSettings : GlobalSettings
{
    [Description("Port to listen for connections")]
    [CommandArgument(0, "<port>")]
    public int Port { get; init; }
    
    [Description("IP address to listen on (default 0.0.0.0)")]
    [CommandOption("-i|--ip-address <value>")]
    public string? IpAddress { get; init; }
}