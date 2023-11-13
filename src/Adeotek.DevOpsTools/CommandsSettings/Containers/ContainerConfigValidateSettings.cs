using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal class ContainerConfigValidateSettings : GlobalSettings
{
    [Description("Config file (with absolute/relative path)")]
    [CommandArgument(0, "<config_file>")]
    public string? ConfigFile { get; init; }
}
