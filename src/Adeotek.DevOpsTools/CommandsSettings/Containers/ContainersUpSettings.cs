using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal sealed class ContainersUpSettings : ContainersSettings
{
    [Description("Execute the command only for the service with the provided name")]
    [CommandOption("-n|--name <value>")]
    public string? ServiceName { get; init; }
    
    [Description("Upgrade container if it exists")]
    [CommandOption("-u|--upgrade")]
    [DefaultValue(false)]
    public bool Upgrade { get; init; }
    
    [Description("Replace existing container, instead of demoting it to 'previous'\nOnly works together with the '--update' option")]
    [CommandOption("-r|--replace")]
    [DefaultValue(false)]
    public bool Replace { get; init; }
    
    [Description("Force container recreation (update), even if the container is on the latest image\nOnly works together with the '--update' option")]
    [CommandOption("-f|--force")]
    [DefaultValue(false)]
    public bool Force { get; init; }
    
    [Description("Backup container volumes before updating\nOnly works together with the '--update' option")]
    [CommandOption("-b|--backup")]
    [DefaultValue(false)]
    public bool Backup { get; init; }
    
    [Description("Volumes backup location (absolute/relative path)")]
    [CommandOption("-l|--backup-location")]
    public string? BackupLocation { get; init; }
}
