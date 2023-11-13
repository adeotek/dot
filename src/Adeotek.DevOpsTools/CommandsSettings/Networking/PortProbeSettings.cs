using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Networking;

public class PortProbeSettings : GlobalSettings
{
    [Description("Target host (hostname or IP address)")]
    [CommandArgument(0, "<host>")]
    public string? Host { get; init; }
    
    [Description("Target Port")]
    [CommandArgument(1, "<port>")]
    public int Port { get; init; }
}