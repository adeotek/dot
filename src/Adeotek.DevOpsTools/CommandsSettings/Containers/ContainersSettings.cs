using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal class ContainersSettings : GlobalSettings
{
    [Description("Config file (with absolute/relative path)")]
    [CommandArgument(0, "<config_file>")]
    public string? ConfigFile { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
    
    [Description("Display loaded configuration")]
    [CommandOption("--show-cfg")]
    [DefaultValue(false)]
    public bool ShowConfig { get; init; }
    
    [Description("Flag for using old configuration file (v1)")]
    [CommandOption("--cfg-v1")]
    public bool? ConfigV1 { get; init; }
}
