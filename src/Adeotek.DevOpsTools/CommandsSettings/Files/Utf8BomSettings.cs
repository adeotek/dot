using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Files;

public class Utf8BomSettings : GlobalSettings
{
    [Description("Target path (absolute or relative)")]
    [CommandArgument(0, "<target_path>")]
    public string TargetPath { get; init; } = default!;

    [Description("Process only files with these extensions (must contain the leading dot, i.e. '.cs')")]
    [CommandOption("-f|--file-ext <value>")]
    public string[] FileExtensions { get; init; } = Array.Empty<string>();
    
    [Description("Subdirectories to be ignored")]
    [CommandOption("-i|--ignore-dir <value>")]
    public string[] IgnoreDirs { get; init; } = Array.Empty<string>();
    
    [Description("Don't apply any changes, just print the commands")]
    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    public bool DryRun { get; init; }
}