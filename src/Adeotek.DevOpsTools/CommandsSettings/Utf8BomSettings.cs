using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

public class Utf8BomSettings : GlobalSettings
{
    [Description("Target directory (with absolute/relative path)")]
    [CommandArgument(0, "<target_directory>")]
    public string? TargetDirectory { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
}