using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

internal class ContainerConfigSampleSettings : GlobalSettings
{
    [Description("Target can be either a new/existing file, or 'display'/'screen' for outputing the sample configuration to the console " +
                 "\nCAUTION: if the target file exists, it will be overiden!")]
    [CommandArgument(0, "<target>")]
    public string? Target { get; init; }
    
    [Description("The format in which the sample config will be generated (YAML/JSON)")]
    [CommandOption("-f|--format")]
    [DefaultValue("yaml")]
    public string? Format { get; init; }
}
