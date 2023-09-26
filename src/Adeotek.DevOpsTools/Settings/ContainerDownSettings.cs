using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Settings;

internal sealed class ContainerDownSettings : ContainerSettings
{
    [Description("Also remove any volumes/networks created for this container")]
    [CommandOption("-p|--purge")]
    [DefaultValue(false)]
    public bool Purge { get; init; }
}
