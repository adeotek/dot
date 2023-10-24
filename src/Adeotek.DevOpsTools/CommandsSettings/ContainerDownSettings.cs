using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

internal sealed class ContainerDownSettings : ContainerSettings
{
    [Description("Downgrade container to the previous version (the last demoted version)\nIt only works when using update with demote")]
    [CommandOption("-d|--downgrade")]
    [DefaultValue(false)]
    public bool Downgrade { get; init; }
    
    [Description("Also remove any volumes/networks created for this container")]
    [CommandOption("-p|--purge")]
    [DefaultValue(false)]
    public bool Purge { get; init; }
    
    [Description("Backup container volumes, before doing any changes")]
    [CommandOption("-b|--backup")]
    [DefaultValue(false)]
    public bool Backup { get; init; }
}
