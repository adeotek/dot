using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

internal class ContainerConfigSampleSettings : ContainerConfigSettings
{
    [Description("The format in which the sample config will be generated (YAML/JSON)")]
    [CommandOption("-f|--format")]
    [DefaultValue("yaml")]
    public string? Format { get; init; }
}
