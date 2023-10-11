using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

public class GlobalSettings : CommandSettings
{
    [Description("Output all messages, including docker commands output")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
    
    [Description("Don't output any messages except errors\n(only outputs 0/1 depending if anything got changed or not)")]
    [CommandOption("-s|--silent")]
    [DefaultValue(false)]
    public bool Silent { get; init; }
}