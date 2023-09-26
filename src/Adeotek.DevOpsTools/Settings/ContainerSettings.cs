using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Settings;

internal class ContainerSettings : CommandSettings
{
    [Description("Config file (with absolute/relative path)")]
    [CommandArgument(0, "<config_file>")]
    public string? ConfigFile { get; init; }
    
    [Description("Don't ask for any user inputs")]
    [CommandOption("-n|--not-interactive|--unattended")]
    [DefaultValue(false)]
    public bool Unattended { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
        
    [Description("Output all messages, including docker commands output")]
    [CommandOption("--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
}
