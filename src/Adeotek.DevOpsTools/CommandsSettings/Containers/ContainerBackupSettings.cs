using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal sealed class ContainerBackupSettings : ContainerSettings
{
    [Description("Execute the command only for the service with the provided name")]
    [CommandOption("-n|--name <value>")]
    public string? Service { get; init; }
}
