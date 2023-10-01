using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

public class Utf8BomSettings : GlobalSettings
{
    [Description("Target path (absolute or relative)")]
    [CommandArgument(0, "<target_path>")]
    public string? TargetPath { get; init; }
    
    [Description("Process only files with these extensions")]
    [CommandOption("-f|--file-extensions")]
    public string? FileExtensions { get; init; }
    
    [Description("Subdirectories to be ignored")]
    [CommandOption("-i|--ignore-dirs")]
    public string? IgnoreDirs { get; init; }
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
}