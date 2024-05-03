using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal class ContainersSettings : GlobalSettings
{
    [Description("Config file (with absolute/relative path)")]
    [CommandArgument(0, "<config_file>")]
    public string? ConfigFile { get; init; }
    
    [Description("Use Podman CLI instead of Docker")]
    [CommandOption("--podman")]
    [DefaultValue(false)]
    public bool UsePodman { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
    
    [Description("Display loaded configuration")]
    [CommandOption("--show-cfg")]
    [DefaultValue(false)]
    public bool ShowConfig { get; init; }
}
