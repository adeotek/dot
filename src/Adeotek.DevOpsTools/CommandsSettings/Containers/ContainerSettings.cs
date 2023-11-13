using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal class ContainerSettings : GlobalSettings
{
    [Description("Config file (with absolute/relative path)")]
    [CommandArgument(0, "<config_file>")]
    public string? ConfigFile { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
}
